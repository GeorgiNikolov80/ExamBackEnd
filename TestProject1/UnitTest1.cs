using NUnit.Framework;
using RestSharp;
using RestSharp.Authenticators;
using System.Net;
using System.Reflection;
using System.Text.Json;
using TestProject1.Models;



namespace TestProject1
{
    [TestFixture]

    public class StoryTests
    {
        private RestClient client;
        private static string createdStoryId;
        private const string baseUrl = "https://d3s5nxhwblsjbi.cloudfront.net/";


        [OneTimeSetUp]
        public void Setup()
        {
            string token = GetJwtToken("Gogo234", "joro12345");

            var options = new RestClientOptions(baseUrl)
            {
                Authenticator = new JwtAuthenticator(token)
            };

            client = new RestClient(options);
        }

        private string GetJwtToken(string username, string password)
        {
            var loginClient = new RestClient(baseUrl);
            var request = new RestRequest("/api/User/Authentication", Method.Post);

            request.AddJsonBody(new { username, password });

            var response = loginClient.Execute(request);
            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);  
            
            return json.GetProperty("accessToken").GetString() ?? string.Empty;
        }

        [Test, Order(1)]
        public void CreatedStory_ShouldReturnCreated()
        {
            var story = new
            {
                title = "New Story",
                description = "A very good story",
                url = ""
            };

            var request = new RestRequest("/api/Story/Create", Method.Post);
            request.AddJsonBody(story);

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));

            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);

            createdStoryId = json.GetProperty("storyId").GetString() ?? string.Empty;

            Assert.That(createdStoryId, Is.Not.Null.Or.Empty, "Story Id should be not null or empty.");
            Assert.That(json.GetProperty("msg").GetString(), Is.EqualTo("Successfully created!"));
        }

        [Test, Order(2)]
        public void EditStory_ShouldReturnOk()
        {
            var editedStory = new StoryDTO
            {
                Title = "New Story",
                Description = "Updated story description",
                Url = ""
            };

            var request = new RestRequest($"/api/Story/Edit/{createdStoryId}", Method.Put);

            request.AddJsonBody(editedStory);

            var response = client.Execute(request);

            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo((HttpStatusCode)HttpStatusCode.OK));
        }

        [Test, Order(3)]
        public void GetAllStory_StoryReturnList()
        {
            var request = new RestRequest("/api/Story/All", Method.Get);
            var response = client.Execute(request); 

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Expected status code 200 OK.");    

            var storys = JsonSerializer.Deserialize<List<object>>(response.Content);

            Assert.That(storys, Is.Not.Empty, "Expected non-empty list of storys.");
        }

        [Test, Order(4)]
        public void DeleteStory_ShouldReturnOk()
        {
            var request = new RestRequest($"/api/Story/Delete/{createdStoryId}", Method.Delete);

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Expected status code 200 OK.");
            Assert.That(response.Content.Contains("Deleted successfully!"));
        }

        [Test, Order(5)]
        public void CreateStory_WithoutRequiredFields_ShouldReturnBadRequest()
        {
            var story = new
            {
                Title = "",
                Description = "This story has no name",
            };

            var request = new RestRequest("/api/Story/Create", Method.Post);
            request.AddJsonBody(story);
            
            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), "Expected staus code 400 Bad request.");
        }

        [Test, Order(6)]
        public void EditNonExistingStory_ShouldReturnNotFound()
        {
            string fakeId = "123";
            var changes = new 
            {
                Title = "New Story",
                Description = "Updated story description",
                Url = ""
            };

            var request = new RestRequest($"/api/Story/Edit/{fakeId}", Method.Put);

            request.AddJsonBody(changes);

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(response.Content.Contains("No spoilers..."));
        }

        [Test, Order(7)]
        public void DeliteNonExistingStory_ShouldReturnBadRequest()
        {
            var request = new RestRequest("/api/Story/Delete/NewStory", Method.Delete);
     var response = client.Execute(request);

     Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
     Assert.That(response.Content, Does.Contain("Unable to delete this story spoiler!"));
        }


















        [OneTimeTearDown]
        public void Cleanup()
        {
            client?.Dispose();
        }
    }
}