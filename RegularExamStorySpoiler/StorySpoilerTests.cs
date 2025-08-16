using RegularExamStorySpoiler.Models;
using RestSharp;
using RestSharp.Authenticators;
using System.Net;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace RegularExamStorySpoiler
{
    public class StorySpoilerTests
    {
        private RestClient client;
        private const string BASEURL = "https://d3s5nxhwblsjbi.cloudfront.net";
        private const string USERNAME = "peyobudakov";
        private const string PASSWORD = "peyobudakov";

        private static string storyID;

        [OneTimeSetUp]
        public void Setup()
        {
            string jwtToken = GetJwtToken(USERNAME, PASSWORD);

            var options = new RestClientOptions(BASEURL)
            {
                Authenticator = new JwtAuthenticator(jwtToken)
            };

            this.client = new RestClient(options);
        }

        private string GetJwtToken(string username, string password)
        {
            RestClient authClient = new RestClient(BASEURL);
            var request = new RestRequest("/api/User/Authentication");
            request.AddJsonBody(new
            {
                username,
                password
            });

            var response = authClient.Execute(request, Method.Post);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = JsonSerializer.Deserialize<JsonElement>(response.Content);
                var token = content.GetProperty("accessToken").GetString();
                if (string.IsNullOrWhiteSpace(token))
                {
                    throw new InvalidOperationException("Access Token is null or empty");
                }
                return token;
            }
            else
            {
                throw new InvalidOperationException($"Unexpected response type {response.StatusCode} with data {response.Content}");
            }
        }

        [OneTimeTearDown] 
        public void TearDown() 
        { 
            client.Dispose();
        }

        [Test, Order(1)]
        public void CreateANewStorySpoiler_WithTheRequiredFields_ShouldSucceed()
        {
            //Arrange
            var newStory = new StoryDTO
            {
                Title = "Story Name",
                Description = "Description",
                Url = ""
            };

            //Act
            var request = new RestRequest("/api/Story/Create", Method.Post);
            request.AddJsonBody(newStory);
            var response = this.client.Execute(request);

            //Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));

            var responseData = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            storyID = responseData.storyId;

            Assert.That(storyID, Is.Not.Null.And.Not.Empty);
            Assert.That(response.Content, Does.Contain("storyId"));

            Assert.That(responseData.Message, Is.EqualTo("Successfully created!"));
            
        }

        [Test, Order(2)]
        public void EditTheCreatedStorySpoiler_ShouldSucceed()
        {
            //Arrange
            var editedStory = new StoryDTO
            {
                Title = "Edited Story Name",
                Description = "Edited Description",
                Url = ""
            };

            //Act
            var request = new RestRequest($"/api/Story/Edit/{storyID}", Method.Put);
            request.AddJsonBody(editedStory);
            var response = this.client.Execute(request);

            //Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var responseData = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);
            Assert.That(responseData.Message, Is.EqualTo("Successfully edited"));
        }

        [Test, Order(3)]
        public void GetAllStorySpoilers_ShouldSucceed()
        {
            //Arrange Act
            var request = new RestRequest("/api/Story/All", Method.Get);
            var response = this.client.Execute(request);

            //Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var responseDataArray = JsonSerializer.Deserialize<ApiResponseDTO[]>(response.Content);
            Assert.That(responseDataArray, Is.Not.Empty);

        }

        [Test, Order(4)]
        public void DeleteAStorySpoiler_ShouldSucceed()
        {
            //Arrange Act
            var request = new RestRequest($"/api/Story/Delete/{storyID}", Method.Delete);
            var response = this.client.Execute(request);

            //Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var responseData = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);
            Assert.That(responseData.Message, Is.EqualTo("Deleted successfully!"));
        }

        [Test, Order(5)]    
        public void CreateAStorySpoiler_WithoutTheRequiredFields_ShouldFail()
        {
            //Arrange
            var newStory = new StoryDTO
            {
                Title = "",
                Description = "",
                Url = ""
            };

            //Act
            var request = new RestRequest("/api/Story/Create", Method.Post);
            request.AddJsonBody(newStory);
            var response = this.client.Execute(request);

            //Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

        }

        [Test, Order(6)]
        public void EditNonExistingStorySpoiler_ShouldFail()
        {
            //Arrange
            var editedStory = new StoryDTO
            {
                Title = "Edited Story Name",
                Description = "Edited Description",
                Url = ""
            };

            //Act
            var request = new RestRequest($"/api/Story/Edit/666", Method.Put);
            request.AddJsonBody(editedStory);
            var response = this.client.Execute(request);

            //Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));

            var responseData = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);
            Assert.That(responseData.Message, Is.EqualTo("No spoilers..."));
        }

        [Test, Order(7)]
        public void DeleteNonExistingStorySpoiler_ShouldFail()
        {
            //Arrange Act
            var request = new RestRequest($"/api/Story/Delete/777", Method.Delete);
            var response = this.client.Execute(request);

            //Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

            var responseData = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);
            Assert.That(responseData.Message, Is.EqualTo("Unable to delete this story spoiler!"));
        }



    }
}