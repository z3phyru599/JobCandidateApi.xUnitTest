using JobCandidateAPI.Models;
using JobCandidateAPI.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JobCandidateApi.xUnitTest
{
    public class JobCandidateControllerTests
    {
        public class CandidateDetailsControllerTests
        {
            private CandidateDetailsController _controller;
            private DbContextOptions<CandidateDetailsContext> _options;

            public CandidateDetailsControllerTests()
            {
                _options = new DbContextOptionsBuilder<CandidateDetailsContext>()
                    .UseInMemoryDatabase(databaseName: "Test_CandidateDetails")
                    .Options;

                using (var context = new CandidateDetailsContext(_options))
                {
                    _controller = new CandidateDetailsController(context);
                }
            }
            [Fact]
            public async Task PostCandidate_WhenCandidateDoesNotExist_CreatesNewCandidate()
            {
                // Arrange
                var newCandidate = new Candidate
                {
                    Email = "test@example.com",
                    FirstName = "John",
                    LastName = "Doe",
                    PhoneNumber = "1234567890",
                    CallIntervalTime = "Anytime",
                    LinkedInURL = "https://linkedin.com/test",
                    GitHubURL = "https://github.com/testuser",
                    Comments = "Test candidate"
                };

                // Act
                var result = await _controller.PostCandidate(newCandidate) as OkObjectResult;

                // Assert
                Assert.NotNull(result);
                Assert.Equal("Candidate added/updated successfully.", result.Value);

                using (var context = new CandidateDetailsContext(_options))
                {
                    var candidateInDb = await context.JobCandidate.FirstOrDefaultAsync(c => c.Email == newCandidate.Email);
                    Assert.NotNull(candidateInDb);
                    Assert.Equal(newCandidate.FirstName, candidateInDb.FirstName);
                    Assert.Equal(newCandidate.LastName, candidateInDb.LastName);
                    Assert.Equal(newCandidate.Email, candidateInDb.Email);
                    Assert.Equal(newCandidate.CallIntervalTime, candidateInDb.CallIntervalTime);
                    Assert.Equal(newCandidate.Comments, candidateInDb.Comments);
                    Assert.Equal(newCandidate.LinkedInURL, candidateInDb.LinkedInURL);
                    Assert.Equal(newCandidate.GitHubURL, candidateInDb.GitHubURL);
                }
            }

            [Fact]
            public async Task PostCandidate_WhenCandidateExists_UpdatesExistingCandidate()
            {
                // Arrange
                var existingCandidate = new Candidate
                {
                    Email = "existing@example.com",
                    FirstName = "Jane",
                    LastName = "Doe",
                    PhoneNumber = "9876543210",
                    CallIntervalTime = "Anytime",
                    LinkedInURL = "https://linkedin.com/existing",
                    GitHubURL = "https://github.com/existinguser",
                    Comments = "Existing candidate"
                };

                using (var context = new CandidateDetailsContext(_options))
                {
                    context.JobCandidate.Add(existingCandidate);
                    await context.SaveChangesAsync();
                }

                existingCandidate.FirstName = "UpdatedJane";
                existingCandidate.PhoneNumber = "9999999999";

                // Act
                var result = await _controller.PostCandidate(existingCandidate) as OkObjectResult;

                // Assert
                Assert.NotNull(result);
                Assert.Equal("Candidate added/updated successfully.", result.Value);

                using (var context = new CandidateDetailsContext(_options))
                {
                    var updatedCandidate = await context.JobCandidate.FirstOrDefaultAsync(c => c.Email == existingCandidate.Email);
                    Assert.NotNull(updatedCandidate);
                    Assert.Equal(existingCandidate.FirstName, updatedCandidate.FirstName);
                    Assert.Equal(existingCandidate.LastName, updatedCandidate.LastName);
                    Assert.Equal(existingCandidate.Email, updatedCandidate.Email);
                    Assert.Equal(existingCandidate.CallIntervalTime, updatedCandidate.CallIntervalTime);
                    Assert.Equal(existingCandidate.Comments, updatedCandidate.Comments);
                    Assert.Equal(existingCandidate.LinkedInURL, updatedCandidate.LinkedInURL);
                    Assert.Equal(existingCandidate.GitHubURL, updatedCandidate.GitHubURL);
                }
            }

            [Fact]
            public async Task PostCandidate_InvalidModelState_ReturnsBadRequest()
            {
                // Arrange
                var invalidCandidate = new Candidate
                {
                    Email = "",
                    FirstName = "",
                    LastName = "",
                    PhoneNumber = "",
                    CallIntervalTime = "",
                    LinkedInURL = "",
                    GitHubURL = "",
                    Comments = ""
                };

                // Act
                var result = await _controller.PostCandidate(invalidCandidate) as BadRequestObjectResult;

                // Assert
                Assert.NotNull(result);
                Assert.IsType<SerializableError>(result.Value);

                var errors = (SerializableError)result.Value;
                Assert.True(errors.ContainsKey("Email"));
                Assert.Contains("The Email field is required.", (string[])errors["Email"]);

                Assert.True(errors.ContainsKey("FirstName"));
                Assert.Contains("The FirstName field is required.", (string[])errors["FirstName"]);

                Assert.True(errors.ContainsKey("LastName"));
                Assert.Contains("The LastName field is required.", (string[])errors["LastName"]);

                Assert.True(errors.ContainsKey("PhoneNumber"));
                Assert.Contains("The PhoneNumber field is required.", (string[])errors["PhoneNumber"]);

                Assert.True(errors.ContainsKey("CallIntervalTime"));
                Assert.Contains("The field CallIntervalTime must be a number.", (string[])errors["CallIntervalTime"]);

                Assert.True(errors.ContainsKey("LinkedInURL"));
                Assert.Contains("The LinkedInURL field is required.", (string[])errors["LinkedInURL"]);

                Assert.True(errors.ContainsKey("GitHubURL"));
                Assert.Contains("The GitHubURL field is required.", (string[])errors["GitHubURL"]);

                Assert.True(errors.ContainsKey("Comments"));
                Assert.Contains("The Comments field is required.", (string[])errors["Comments"]);
            }

            [Fact]
            public async Task PostCandidate_ExceptionThrown_ReturnsInternalServerError()
            {
                // Arrange
                var candidate = new Candidate
                {
                    Email = "test@example.com",
                    FirstName = "John",
                    LastName = "Doe",
                    PhoneNumber = "1234567890",
                    CallIntervalTime = "Anytime",
                    LinkedInURL = "https://linkedin.com/test",
                    GitHubURL = "https://github.com/testuser",
                    Comments = "Test candidate"
                };

                _controller = new CandidateDetailsController(null);

                // Act
                var result = await _controller.PostCandidate(candidate) as StatusCodeResult;

                // Assert
                Assert.NotNull(result);
                Assert.Equal(500, result.StatusCode);
            }

            [Fact]
            public async Task PostCandidate_EmptyFields_UpdatesOnlyNonEmptyFields()
            {
                // Arrange
                var existingCandidate = new Candidate
                {
                    Email = "existing@example.com",
                    FirstName = "Jane",
                    LastName = "Doe",
                    PhoneNumber = "9876543210",
                    CallIntervalTime = "Anytime",
                    LinkedInURL = "https://linkedin.com/existing",
                    GitHubURL = "https://github.com/existinguser",
                    Comments = "Existing candidate"
                };

                using (var context = new CandidateDetailsContext(_options))
                {
                    context.JobCandidate.Add(existingCandidate);
                    await context.SaveChangesAsync();
                }

                var updatedCandidate = new Candidate
                {
                    Email = "existing@example.com",
                    FirstName = "", 
                    LastName = null, 
                    PhoneNumber = "9999999999",
                    CallIntervalTime = null, 
                    LinkedInURL = "", 
                    GitHubURL = null, 
                    Comments = ""
                };

                // Act
                var result = await _controller.PostCandidate(updatedCandidate) as OkObjectResult;

                // Assert
                Assert.NotNull(result);
                Assert.Equal("Candidate added/updated successfully.", result.Value);


                using (var context = new CandidateDetailsContext(_options))
                {
                    var updatedCandidateInDb = await context.JobCandidate.FirstOrDefaultAsync(c => c.Email == existingCandidate.Email);
                    Assert.NotNull(updatedCandidateInDb);
                    Assert.Equal(existingCandidate.FirstName, updatedCandidateInDb.FirstName);
                    Assert.Equal(updatedCandidate.PhoneNumber, updatedCandidateInDb.PhoneNumber);
                }
            }

            [Fact]
            public async Task PostCandidate_MaximumStringLength_UpdatesCandidate()
            {
                // Arrange
                var existingCandidate = new Candidate
                {
                    Email = "existing@example.com",
                    FirstName = "Jane",
                    LastName = "Doe",
                    PhoneNumber = "9876543210",
                    CallIntervalTime = "Anytime",
                    LinkedInURL = "https://linkedin.com/existing",
                    GitHubURL = "https://github.com/existinguser",
                    Comments = "Existing candidate"
                };

                using (var context = new CandidateDetailsContext(_options))
                {
                    context.JobCandidate.Add(existingCandidate);
                    await context.SaveChangesAsync();
                }

                var updatedCandidate = new Candidate
                {
                    Email = "existing@example.com",
                    FirstName = new string('A', 50),
                    LastName = new string('B', 50),
                    PhoneNumber = "9999999999",
                    CallIntervalTime = "Anytime",
                    LinkedInURL = new string('C', 200),
                    GitHubURL = new string('D', 200), 
                    Comments = new string('E', 1000) 
                };

                // Act
                var result = await _controller.PostCandidate(updatedCandidate) as OkObjectResult;

                // Assert
                Assert.NotNull(result);
                Assert.Equal("Candidate added/updated successfully.", result.Value);

                using (var context = new CandidateDetailsContext(_options))
                {
                    var updatedCandidateInDb = await context.JobCandidate.FirstOrDefaultAsync(c => c.Email == existingCandidate.Email);
                    Assert.NotNull(updatedCandidateInDb);
                    Assert.Equal(updatedCandidate.FirstName, updatedCandidateInDb.FirstName);
                    Assert.Equal(updatedCandidate.LastName, updatedCandidateInDb.LastName);
                    Assert.Equal(updatedCandidate.PhoneNumber, updatedCandidateInDb.PhoneNumber);
                    Assert.Equal(updatedCandidate.CallIntervalTime, updatedCandidateInDb.CallIntervalTime);
                    Assert.Equal(updatedCandidate.LinkedInURL, updatedCandidateInDb.LinkedInURL);
                    Assert.Equal(updatedCandidate.GitHubURL, updatedCandidateInDb.GitHubURL);
                    Assert.Equal(updatedCandidate.Comments, updatedCandidateInDb.Comments);
                }
            }
        }
    }
}