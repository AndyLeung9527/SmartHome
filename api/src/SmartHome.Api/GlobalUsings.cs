﻿global using Asp.Versioning;
global using Asp.Versioning.ApiExplorer;
global using EventBusRabbitMQ;
global using IdGen;
global using IdGen.DependencyInjection;
global using MediatR;
global using Microsoft.AspNetCore.Authentication.JwtBearer;
global using Microsoft.AspNetCore.Http.HttpResults;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.AspNetCore.SignalR;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.Extensions.Primitives;
global using Microsoft.IdentityModel.Tokens;
global using Scalar.AspNetCore;
global using SmartHome.Api.Apis;
global using SmartHome.Api.Application.Commands;
global using SmartHome.Api.Dtos;
global using SmartHome.Api.HostApplicationBuilderExtensions;
global using SmartHome.Api.Hubs;
global using SmartHome.Domain.AggregatesModel.BroadcastAggregate;
global using SmartHome.Domain.AggregatesModel.UserAggregate;
global using SmartHome.Domain.Events;
global using SmartHome.Infrastructure;
global using SmartHome.Infrastructure.Repositories;
global using System.Net;
global using System.Security.Claims;
global using System.Security.Cryptography;
global using System.Text;
