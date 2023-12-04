using FisherTournament.Application.Common.Resources;
using FisherTournament.Application.Competitions.Commands.AddParticipation;
using FisherTournament.Application.Competitions.Commands.AddScore;
using FisherTournament.Application.Competitions.Commands.EditCompetition;
using FisherTournament.Application.Competitions.Queries.GetLeaderBoard;
using FisherTournament.Application.Tournaments.Commands.AddCompetitions;
using FisherTournament.Contracts.Competitions;
using FisherTournament.Domain.CompetitionAggregate;
using Mapster;

namespace FisherTournament.Infrastracture.Common.Mapping;

public class CompetitionMapping : IRegister
{
	static CompetitionLocationResource? MapLocation(CreateLocationRequest? src)
	{
		return src is null ? null : new CompetitionLocationResource(src.City, src.State, src.Country, src.Place);
	}

	static EditCompetitionCommand MapEditRequest((EditCompetitionRequest RQ, string competitionId) src)
	{
		return new EditCompetitionCommand(
			src.competitionId,
			src.RQ.StartDateTime,
			src.RQ.Location?.Adapt<CompetitionLocationResource>(),
			src.RQ.CompetitionFinishedState);
	}

	public void Register(TypeAdapterConfig config)
	{

		config.NewConfig<AddCompetitionRequest, AddCompetitionCommand>();

		//config.NewConfig<CreateLocationRequest, CompetitionLocationResource>();
		config.NewConfig<CreateLocationRequest, CompetitionLocationResource>()
			.MapWith(src => new CompetitionLocationResource(src.City, src.State, src.Country, src.Place));

		config.NewConfig<CreateLocationRequest?, CompetitionLocationResource?>()
			.MapWith(src => MapLocation(src));

		config.NewConfig<(AddCompetitionsRequest CR, string tournamentId), AddCompetitionsCommand>()
			.Map(dest => dest.TournamentId, src => src.tournamentId)
			.Map(dest => dest.Competitions, src => src.CR.Competitions);

		config.NewConfig<(AddScoreRequest SR, string competitionId), AddScoreCommand>()
			.Map(dest => dest.CompetitionId, src => src.competitionId)
			.Map(dest => dest.FisherId, src => src.SR.FisherId)
			.Map(dest => dest.Score, src => src.SR.Score);

		config.NewConfig<Competition, CompetitionResponse>();
		config.NewConfig<List<Competition>, AddCompetitionsResponse>()
			.Map(dest => dest.Competitions, src => src);

		config.NewConfig<LeaderBoardCategory, CompetitionCategoryLeaderBoard>()
			.Map(dest => dest.CategoryName, src => src.Name)
			.Map(dest => dest.CategoryId, src => src.Id)
			.Map(dest => dest.LeaderBoard, src => src.LeaderBoard);

		config.NewConfig<(AddParticipationRequest SR, string competitionId), AddParticipationCommand>()
			.Map(dest => dest.CompetitionId, src => src.competitionId)
			.Map(dest => dest.FisherId, src => src.SR.FisherId);


		config.NewConfig<(EditCompetitionRequest RQ, string competitionId), EditCompetitionCommand>()
		.MapWith(src => MapEditRequest(src));
	}
}