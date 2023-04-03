using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FisherTournament.API.Common.CustomResults;
using FisherTournament.Application.Tournaments.Commands.AddCategory;
using FisherTournament.Contracts.Categories;
using MapsterMapper;
using MediatR;
using MinimalApi.Endpoint;

namespace FisherTournament.API.Endpoints.Categories
{
    public class AddCategoryEndpoint : IEndpoint<IResult, AddCategoryRequest, string, ISender>
    {
        private readonly IMapper _mapper;

        public AddCategoryEndpoint(IMapper mapper)
        {
            _mapper = mapper;
        }

        public void AddRoute(IEndpointRouteBuilder app)
        {
            app.MapPost("/tournaments/{tournamentId}/categories",
            async (AddCategoryRequest cmd,
                   string tournamentId,
                   ISender sender) => await HandleAsync(cmd, tournamentId, sender))
                .WithTags("CategoriesEndpoints")
                .WithOpenApi(cfg => new(cfg)
                {
                    Summary = "Add a new category to a tournament",
                })
                .WithName("AddCategory")
                .Produces<AddCategoryResponse>(StatusCodes.Status200OK)
                .ProducesValidationProblem()
                .ProducesProblem(StatusCodes.Status409Conflict);
        }

        public async Task<IResult> HandleAsync(AddCategoryRequest request,
                                               string tournamentId,
                                               ISender sender)
        {
            var command = _mapper.Map<AddCategoryCommand>((request, tournamentId));
            var result = await sender.Send(command);
            return result.Match(
                value => Results.Ok(_mapper.Map<AddCategoryResponse>(value)),
                errors => Results.Extensions.Problem(errors)
            );
        }
    }
}