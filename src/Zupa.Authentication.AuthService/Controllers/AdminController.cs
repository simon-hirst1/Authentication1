using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Entities;
using IdentityServer4.EntityFramework.Mappers;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Zupa.Authentication.AuthService.AppInsights;
using Zupa.Authentication.AuthService.Configuration;
using Zupa.Authentication.AuthService.Models.Admin;
using Zupa.Authentication.Common;
using ApiResourceModel = IdentityServer4.Models.ApiResource;
using ClientModel = IdentityServer4.Models.Client;
using IdentityResourceEntity = IdentityServer4.EntityFramework.Entities.IdentityResource;
using IdentityResourceModel = IdentityServer4.Models.IdentityResource;
using SecretModel = IdentityServer4.Models.Secret;

namespace Zupa.Authentication.AuthService.Controllers
{
    [Authorize(Roles = RoleConstants.AdminRole)]
    public class AdminController : Controller
    {
        private readonly ConfigurationDbContext _configurationDbContext;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ITrackTelemetry _trackTelemetry;
        private readonly MessageConstants _messageConstants;

        private readonly Dictionary<string, IdentityResourceModel> _identityResources =
            new Dictionary<string, IdentityResourceModel>
            {
                {new IdentityResources.Address().Name, new IdentityResources.Address()},
                {new IdentityResources.Email().Name, new IdentityResources.Email()},
                {new IdentityResources.OpenId().Name, new IdentityResources.OpenId()},
                {new IdentityResources.Phone().Name, new IdentityResources.Phone()},
                {new IdentityResources.Profile().Name, new IdentityResources.Profile()}
            };

        public AdminController(ConfigurationDbContext configurationDbContext,
                UserManager<IdentityUser> userManager,
                ITrackTelemetry trackTelemetry,
                IOptions<MessageConstants> messageConstants)
        {
            _configurationDbContext = configurationDbContext ?? throw new ArgumentNullException(nameof(configurationDbContext));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _trackTelemetry = trackTelemetry ?? throw new ArgumentNullException(nameof(trackTelemetry));
            _messageConstants = messageConstants?.Value ?? throw new ArgumentNullException(nameof(messageConstants));
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _trackTelemetry.TrackEvent(EventName.AdminManagement, EventType.Action, EventStatus.UserNotFound);
                return BadRequest(_messageConstants.RequestUnsuccessful);
            }

            var clients = _configurationDbContext.Clients.ToList();
            var apiResources = _configurationDbContext.ApiResources.ToList();
            var registeredIdentityResources = _configurationDbContext.IdentityResources.ToList();

            var model = new AdminViewModel
            {
                Clients = clients,
                ApiResources = apiResources,
                RegisteredIdentityResources = registeredIdentityResources,
                IdentityResources = _identityResources.Values,
                IsRemoveButtonVisibleForClients = clients.Any(),
                IsRemoveButtonVisibleForApiResources = apiResources.Any()
            };

            return View(model);
        }

        [HttpGet]
        public IActionResult AddClient()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddClient(ClientViewModel clientViewModel)
        {
            if (!ModelState.IsValid)
                return View();

            var apiScopes = clientViewModel.ApiScopes.Split(" ").Select(apiScope => apiScope.Trim()).ToArray();

            var client = new ClientModel
            {
                ClientId = clientViewModel.ClientId,
                ClientName = clientViewModel.ClientName,
                AllowedGrantTypes = GrantTypes.Implicit,
                AllowAccessTokensViaBrowser = clientViewModel.AllowAccessTokensViaBrowser,
                RequireConsent = false,

                RedirectUris = { clientViewModel.RedirectUri },
                PostLogoutRedirectUris = { clientViewModel.PostLogoutRedirectUri },
                AllowedCorsOrigins = { clientViewModel.CorsOrigin },

                AllowedScopes = apiScopes
            };

            _configurationDbContext.Clients.Add(client.ToEntity());
            _configurationDbContext.SaveChanges();

            _trackTelemetry.TrackEvent(EventName.AddClient, EventType.Action, EventStatus.Success);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult AddTestClient()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddTestClient(TestClientViewModel clientViewModel)
        {
            if (!ModelState.IsValid)
                return View();

            var apiScopes = clientViewModel.ApiScopes.Split(" ").Select(apiScope => apiScope.Trim()).ToArray();

            var client = new ClientModel
            {
                ClientId = clientViewModel.ClientId,
                ClientName = clientViewModel.ClientName,
                AllowedGrantTypes = GrantTypes.ResourceOwnerPasswordAndClientCredentials,

                ClientSecrets =
                {
                    new SecretModel(clientViewModel.ClientSecret.Sha256())
                },

                AllowedScopes = apiScopes
            };

            _configurationDbContext.Clients.Add(client.ToEntity());
            _configurationDbContext.SaveChanges();

            _trackTelemetry.TrackEvent(EventName.AddTestClient, EventType.Action, EventStatus.Success);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> UpdateClient(int id)
        {
            if (id <= 0)
            {
                _trackTelemetry.TrackEvent(EventName.UpdateClient, EventType.Action, EventStatus.InvalidParameter);
                return BadRequest(_messageConstants.RequestUnsuccessful);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _trackTelemetry.TrackEvent(EventName.UpdateClient, EventType.Action, EventStatus.UserNotFound);
                return BadRequest(_messageConstants.RequestUnsuccessful);
            }

            var client = _configurationDbContext.Clients.AsQueryable()
                .Include(c => c.AllowedScopes)
                .Include(c => c.AllowedCorsOrigins)
                .Include(c => c.PostLogoutRedirectUris)
                .Include(c => c.RedirectUris)
                .Include(c => c.ClientSecrets)
                .Single(c => c.Id == id);

            if (!client.AllowedCorsOrigins.Any() && !client.PostLogoutRedirectUris.Any() && !client.RedirectUris.Any() && client.ClientSecrets != null)
                return RedirectToAction(nameof(UpdateTestClient), client);

            _trackTelemetry.TrackEvent(EventName.UpdateClient, EventType.Action, EventStatus.Success);

            return View(new ClientViewModel
            {
                Id = client.Id,
                ClientId = client.ClientId,
                ClientName = client.ClientName,
                AllowAccessTokensViaBrowser = client.AllowAccessTokensViaBrowser,
                CorsOrigin = string.Join("; ", client.AllowedCorsOrigins.Select(aco => aco.Origin)),
                PostLogoutRedirectUri = string.Join(", ", client.PostLogoutRedirectUris.Select(plru => plru.PostLogoutRedirectUri)),
                RedirectUri = string.Join(", ", client.RedirectUris.Select(ru => ru.RedirectUri)),
                ApiScopes = string.Join(" ", client.AllowedScopes.Select(a => a.Scope))
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateClient(ClientViewModel clientViewModel)
        {
            if (!ModelState.IsValid)
            {
                _trackTelemetry.TrackEvent(EventName.UpdateClient, EventType.Action, EventStatus.Fail);
                return View();
            }

            var corsOrigins = clientViewModel.CorsOrigin.Split("; ").Select(origin => origin.Trim()).ToList();
            var postLogoutRedirectUris = clientViewModel.PostLogoutRedirectUri.Split(", ").Select(uri => uri.Trim()).ToList();
            var redirectUris = clientViewModel.RedirectUri.Split(", ").Select(uri => uri.Trim()).ToList();
            var apiScopes = clientViewModel.ApiScopes.Split(" ").Select(apiScope => apiScope.Trim()).ToList();

            var clientEntity = _configurationDbContext.Clients.AsQueryable()
                .Include(c => c.AllowedScopes)
                .Include(c => c.AllowedCorsOrigins)
                .Include(c => c.PostLogoutRedirectUris)
                .Include(c => c.RedirectUris)
                .Single(c => c.Id == clientViewModel.Id);
            clientEntity.ClientId = clientViewModel.ClientId;
            clientEntity.ClientName = clientViewModel.ClientName;
            clientEntity.AllowAccessTokensViaBrowser = clientViewModel.AllowAccessTokensViaBrowser;
            clientEntity.AllowedCorsOrigins = corsOrigins.Select(origin => new ClientCorsOrigin
            {
                Client = clientEntity,
                Origin = origin
            }).ToList();
            clientEntity.PostLogoutRedirectUris = postLogoutRedirectUris.Select(uri => new ClientPostLogoutRedirectUri
            {
                Client = clientEntity,
                PostLogoutRedirectUri = uri
            }).ToList();
            clientEntity.RedirectUris = redirectUris.Select(uri => new ClientRedirectUri
            {
                Client = clientEntity,
                RedirectUri = uri
            }).ToList();
            clientEntity.AllowedScopes = apiScopes.Select(scope => new ClientScope
            {
                Client = clientEntity,
                Scope = scope
            }).ToList();

            _configurationDbContext.Clients.Update(clientEntity);
            _configurationDbContext.SaveChanges();

            _trackTelemetry.TrackEvent(EventName.UpdateClient, EventType.Action, EventStatus.Success);

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult UpdateTestClient(int id)
        {
            if (id <= 0)
            {
                _trackTelemetry.TrackEvent(EventName.UpdateTestClient, EventType.Action, EventStatus.InvalidParameter);
                return BadRequest(_messageConstants.RequestUnsuccessful);
            }

            var client = _configurationDbContext.Clients.AsQueryable()
                .Include(c => c.AllowedScopes)
                .Single(c => c.Id == id);

            _trackTelemetry.TrackEvent(EventName.UpdateTestClient, EventType.Action, EventStatus.Success);

            return View(new TestClientViewModel
            {
                Id = client.Id,
                ClientId = client.ClientId,
                ClientName = client.ClientName,
                ApiScopes = string.Join(" ", client.AllowedScopes.Select(a => a.Scope))
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateTestClient(TestClientViewModel clientViewModel)
        {
            if (!ModelState.IsValid)
            {
                _trackTelemetry.TrackEvent(EventName.UpdateTestClient, EventType.Action, EventStatus.Fail);
                return View();
            }

            var apiScopes = clientViewModel.ApiScopes.Split(" ").Select(apiScope => apiScope.Trim()).ToList();

            var clientEntity = _configurationDbContext.Clients.AsQueryable()
                .Include(c => c.AllowedScopes)
                .Single(c => c.Id == clientViewModel.Id);

            clientEntity.ClientId = clientViewModel.ClientId;
            clientEntity.ClientName = clientViewModel.ClientName;
            clientEntity.AllowedScopes = apiScopes.Select(scope => new ClientScope
            {
                Client = clientEntity,
                Scope = scope
            }).ToList();

            _configurationDbContext.Clients.Update(clientEntity);
            _configurationDbContext.SaveChanges();

            _trackTelemetry.TrackEvent(EventName.UpdateTestClient, EventType.Action, EventStatus.Success);

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveClient(int id)
        {
            if (id <= 0)
            {
                _trackTelemetry.TrackEvent(EventName.RemoveClient, EventType.Action, EventStatus.InvalidParameter);
                return BadRequest(_messageConstants.RequestUnsuccessful);
            }
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _trackTelemetry.TrackEvent(EventName.RemoveClient, EventType.Action, EventStatus.UserNotFound);
                return BadRequest(_messageConstants.RequestUnsuccessful);
            }

            var clientEntity = _configurationDbContext.Clients.Find(id);
            _configurationDbContext.Clients.Remove(clientEntity);
            _configurationDbContext.SaveChanges();

            _trackTelemetry.TrackEvent(EventName.RemoveClient, EventType.Action, EventStatus.Success);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult AddApiResource()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddApiResource(ApiResourceViewModel apiResourceViewModel)
        {
            if (!ModelState.IsValid)
                return View();

            var apiResource = new ApiResourceModel(apiResourceViewModel.Name, apiResourceViewModel.DisplayName);

            _configurationDbContext.ApiResources.Add(apiResource.ToEntity());
            _configurationDbContext.SaveChanges();

            _trackTelemetry.TrackEvent(EventName.AddApiResource, EventType.Action, EventStatus.Success);
            return RedirectToAction(nameof(Index));
        }


        [HttpGet]
        public async Task<IActionResult> UpdateApiResource(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                _trackTelemetry.TrackEvent(EventName.UpdateApiResource, EventType.Action, EventStatus.ParameterNotFound);
                return BadRequest(_messageConstants.RequestUnsuccessful);
            }
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _trackTelemetry.TrackEvent(EventName.UpdateApiResource, EventType.Action, EventStatus.UserNotFound);
                return BadRequest(_messageConstants.RequestUnsuccessful);
            }

            var apiResource = _configurationDbContext.ApiResources.Single(a => a.Name == name);

            _trackTelemetry.TrackEvent(EventName.UpdateApiResource, EventType.Action, EventStatus.Success);

            return View(new ApiResourceViewModel
            {
                Name = apiResource.Name,
                DisplayName = apiResource.DisplayName
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateApiResource(ApiResourceViewModel apiResourceViewModel)
        {
            if (!ModelState.IsValid)
            {
                _trackTelemetry.TrackEvent(EventName.UpdateApiResource, EventType.Action, EventStatus.Fail);
                return View();
            }

            var apiResource = _configurationDbContext.ApiResources.Single(c => c.Name == apiResourceViewModel.Name);

            apiResource.DisplayName = apiResourceViewModel.DisplayName;

            _configurationDbContext.ApiResources.Update(apiResource);
            _configurationDbContext.SaveChanges();

            _trackTelemetry.TrackEvent(EventName.UpdateApiResource, EventType.Action, EventStatus.Success);

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveApiResource(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                _trackTelemetry.TrackEvent(EventName.RemoveApiResource, EventType.Action, EventStatus.ParameterNotFound);
                return BadRequest(_messageConstants.RequestUnsuccessful);
            }
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _trackTelemetry.TrackEvent(EventName.RemoveApiResource, EventType.Action, EventStatus.UserNotFound);
                return BadRequest(_messageConstants.RequestUnsuccessful);
            }

            var apiResourceEntity = _configurationDbContext.ApiResources.Single(c => c.Name == name);
            _configurationDbContext.ApiResources.Remove(apiResourceEntity);
            _configurationDbContext.SaveChanges();

            _trackTelemetry.TrackEvent(EventName.RemoveApiResource, EventType.Action, EventStatus.Success);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddIdentityResource(IdentityResourceModel identityResource)
        {
            if (identityResource == null)
            {
                _trackTelemetry.TrackEvent(EventName.AddIdentityResource, EventType.Action, EventStatus.ParameterNotFound);
                return BadRequest(_messageConstants.RequestUnsuccessful);
            }
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _trackTelemetry.TrackEvent(EventName.AddIdentityResource, EventType.Action, EventStatus.UserNotFound);
                return BadRequest(_messageConstants.RequestUnsuccessful);
            }

            _identityResources.TryGetValue(identityResource.Name, out var resource);

            if (resource == null)
            {
                _trackTelemetry.TrackEvent(EventName.AddIdentityResource, EventType.Action, EventStatus.ParameterNotFound);
                return BadRequest(_messageConstants.RequestUnsuccessful);
            }

            _configurationDbContext.IdentityResources.Add(resource.ToEntity());
            _configurationDbContext.SaveChanges();

            _trackTelemetry.TrackEvent(EventName.AddIdentityResource, EventType.Action, EventStatus.Success);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveIdentityResource(IdentityResourceEntity identityResource)
        {
            if (identityResource == null)
            {
                _trackTelemetry.TrackEvent(EventName.RemoveIdentityResource, EventType.Action, EventStatus.ParameterNotFound);
                return BadRequest(_messageConstants.RequestUnsuccessful);
            }
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _trackTelemetry.TrackEvent(EventName.RemoveIdentityResource, EventType.Action, EventStatus.UserNotFound);
                return BadRequest(_messageConstants.RequestUnsuccessful);
            }

            var identityResourceEntity = _configurationDbContext.IdentityResources.First(c => c.Id == identityResource.Id);
            _configurationDbContext.IdentityResources.Remove(identityResourceEntity);
            _configurationDbContext.SaveChanges();

            _trackTelemetry.TrackEvent(EventName.RemoveIdentityResource, EventType.Action, EventStatus.Success);
            return RedirectToAction(nameof(Index));
        }
    }
}
