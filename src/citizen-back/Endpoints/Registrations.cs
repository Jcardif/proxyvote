using System;
using System.Net;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

using ProxyVote.Core.Entities;
using ProxyVote.Core.Services;
using ProxyVote.IdentityAuthority.Core;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;

namespace ProxyVote.Citizen.Back.Endpoints;

public class Registrations
{
    private readonly ProxyRegistrationService _registrationService;
    private readonly RegistrationIdentityService _registrationIdentityService;

    public Registrations(ProxyRegistrationService registrationService, RegistrationIdentityService registrationIdentityService)
    {
        _registrationService = registrationService;
        _registrationIdentityService = registrationIdentityService;
    }

    [Function(nameof(SubmitRegistration))]
    [OpenApiOperation(nameof(SubmitRegistration), tags: new[] { "application" }, Description = "Insert a new proxy application in the system.")]
    [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(ProxyApplication), Required = true)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Description = "The OK response")]
    public async Task<IActionResult> SubmitRegistration(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "application")] ProxyApplication application,
        ILogger log)
    {
        log.LogInformation($"{nameof(SubmitRegistration)} called.");

        var registrationId = await _registrationService.CreateRegistrationAsync(application);
        
        return new CreatedResult("/registrations", new { Id = registrationId });
    }


    [Function(nameof(GetRegistrationById))]
    [OpenApiOperation(nameof(GetRegistrationById), tags: new[] { "application" }, Description = "Get a application by Id.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(ProxyApplication), Description = "The requested application")]
    [OpenApiParameter("department",In = ParameterLocation.Path, Required = true, Type = typeof(string))]
    [OpenApiParameter("registrationId", In = ParameterLocation.Path, Required = true, Type = typeof(string))]
    public async Task<IActionResult> GetRegistrationById(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "application/{department}/{registrationId}")]
        HttpRequest req,
        string registrationId,
        string department,
        ILogger log)
    {
        var registration = await _registrationIdentityService.GetRegistrationById(department, registrationId);
        return registration == null ? new NotFoundResult() : new OkObjectResult(registration);
    }



    [Function(nameof(ValidateRegistration))]
    [OpenApiOperation(nameof(ValidateRegistration), tags: new[] { "application" }, Description = "Validate a specific application.")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.OK)]
    [OpenApiParameter("department", In = ParameterLocation.Path, Required = true, Type = typeof(string))]
    [OpenApiParameter("registrationId", In = ParameterLocation.Path, Required = true, Type = typeof(string))]
    [OpenApiRequestBody("application/json", typeof(ApplicationValidation))]
    public async Task<IActionResult> ValidateRegistration(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "application/{department}/{registrationId}/validate")]
        ApplicationValidation validation,
        string registrationId,
        string department,
        ILogger log)
    {
        await _registrationIdentityService.ValidateRegistration(department, registrationId, validation);
        return new OkResult();
    }







    [Function(nameof(TestRegistration))]
    [OpenApiOperation(nameof(TestRegistration), tags: new[] { "application" }, Description = "Test.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(ProxyApplication), Description = "The OK response")]
    public IActionResult TestRegistration(
    [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "registrationtest")] ProxyApplication application,
    ILogger log)
    {
        log.LogInformation($"{nameof(TestRegistration)} called.");

        return new OkObjectResult(new ProxyApplication()
        {
            RegistrationId = Guid.NewGuid().ToString(),
            CreatedAt = DateTime.UtcNow.AddDays(-2),
            ValidUntil = DateTime.UtcNow.AddDays(200),
            Applicant = new Applicant()
            {
                FirstName = "Jean",
                LastName = "Valjean",
                BirthDate = DateTime.UtcNow.AddYears(-35),
                EmailAddress = "demo@demo.com",
                CityName = "Paris",
                PostalCode = "75001",
                State = "Ile de France",
                StreetAddress = "39 Quai du Président Roosevelt"
            },

            ProxyVoter = new ProxyVoter()
            {
                FirstName = "Henri",
                LastName = "Dole",
                BirthDate = DateTime.UtcNow.AddYears(-42),
            }
            
        });
    }
}

