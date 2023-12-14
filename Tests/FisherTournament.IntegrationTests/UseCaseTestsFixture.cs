using FisherTournament.Infrastracture.Persistence.Tournaments;

namespace FisherTournament.IntegrationTests;

using FisherTournament.Application;
using FisherTournament.Application.Common.Persistence;
using FisherTournament.Application.LeaderBoard;
using FisherTournament.Domain.Common.Provider;
using FisherTournament.Domain.CompetitionAggregate.ValueObjects;
using FisherTournament.Domain.FisherAggregate.ValueObjects;
using FisherTournament.Domain.TournamentAggregate.ValueObjects;
using FisherTournament.Infrastracture;
using FisherTournament.Infrastracture.Persistence.ReadModels.EntityFramework;
using FisherTournament.Infrastracture.Persistence.ReadModels.EntityFramework.Repositories;
using FisherTournament.ReadModels.Models;
using FisherTournament.ReadModels.Persistence;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using System.Collections.Generic;
using System.Diagnostics;

public class UseCaseTestsFixture : IDisposable
{
	private readonly IServiceScopeFactory _scopeFactory;

	public IDateTimeProvider DateTimeProvider => _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<IDateTimeProvider>();

	public Mock<IDateTimeProvider> DateTimeProviderMock = new();

	public TournamentFisherDbContext TournamentContext => (_scopeFactory.CreateScope().ServiceProvider.GetRequiredService<ITournamentFisherDbContext>() as TournamentFisherDbContext)!;

	public ReadModelsDbContext ReadModelsContext => _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<ReadModelsDbContext>()!;

	public ILeaderBoardRepository LeaderBoardRepository => _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<ILeaderBoardRepository>();

	// Set this property to mock the LeaderBoardRepository
	public Mock<ILeaderBoardRepository>? LeaderboardRepositoryMock = null;

	public UseCaseTestsFixture()
	{
		var builder = Host.CreateDefaultBuilder();

		var configuration = new ConfigurationBuilder()
			.AddEnvironmentVariables()
			.AddJsonFile("appsettings.json")
			.Build();

		builder.ConfigureAppConfiguration(builder =>
		{
			builder.Sources.Clear();
			builder.AddConfiguration(configuration);
		});

		builder.ConfigureServices((hostContext, services) =>
		{
			services.AddApplication();
			services.AddInfrastructure();
			services.AddSettings(configuration);

			DateTimeProviderMock.Setup(x => x.Now).Returns(() => DateTime.UtcNow);
			services.AddSingleton<IDateTimeProvider>(DateTimeProviderMock.Object);

			// add a ActivitySource, just so it can be resolved
			services.AddSingleton(new ActivitySource("Test"));

			// mock the leader board repository
			// By default it forwards to the real implementation LeaderBoardRepository
			services.AddScoped<ILeaderBoardRepository>(s =>
			 new LeaderBoardRepositoryDispatcher(LeaderboardRepositoryMock, new LeaderBoardRepository(s.GetRequiredService<ReadModelsDbContext>())));
		});


		var host = builder.Build();

		_scopeFactory = host.Services.GetRequiredService<IServiceScopeFactory>();

		TournamentFisherDbContext tournamentContext = (host.Services.GetRequiredService<ITournamentFisherDbContext>() as TournamentFisherDbContext)!;
		ReadModelsDbContext readModelsDbContext = host.Services.GetRequiredService<ReadModelsDbContext>()!;

		tournamentContext.Database.EnsureDeleted();
		tournamentContext.Database.EnsureCreated();

		readModelsDbContext.Database.EnsureDeleted();
		readModelsDbContext.Database.EnsureCreated();
	}

	public void Dispose()
	{
		LeaderboardRepositoryMock = null;
	}

	public async Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request)
	{
		using (var scope = _scopeFactory.CreateScope())
		{
			var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
			return await mediator.Send(request);
		}
	}

	/// <summary>
	/// Executes all pending leader board jobs.
	/// </summary>
	/// <returns>The number of executed jobs.</returns>
	public async Task<int> ExecutePendingLeaderBoardJobs()
	{
		using var scope = _scopeFactory.CreateScope();

		var scheduler = scope.ServiceProvider.GetRequiredService<ILeaderBoardUpdateScheduler>();
		var updater = scope.ServiceProvider.GetRequiredService<ILeaderBoardUpdater>();

		// fwd 5m
		DateTimeProviderMock.Setup(x => x.Now).Returns(() => DateTime.UtcNow.AddMinutes(5));

		int executed = 0;
		while (scheduler.GetNextJob() is var job && job != null)
		{
			await updater.UpdateLeaderBoard(job.TournamentId, job.CategoryId, job.CompetitionsToUpdate);
			executed++;
		}

		DateTimeProviderMock.Setup(x => x.Now).Returns(() => DateTime.UtcNow); // reset

		return executed;
	}
}

public class LeaderBoardRepositoryDispatcher : ILeaderBoardRepository
{
	public Mock<ILeaderBoardRepository>? RepositoryMock;
	private readonly ILeaderBoardRepository _repository;

	public LeaderBoardRepositoryDispatcher(Mock<ILeaderBoardRepository>? repositoryMock, ILeaderBoardRepository repository)
	{
		this.RepositoryMock = repositoryMock;
		this._repository = repository;
	}

	private ILeaderBoardRepository CurrentRepository
	{
		get
		{
			return RepositoryMock is null ? this._repository : RepositoryMock.Object;
		}
	}

	public void AddCompetitionLeaderBoardItem(LeaderboardCompetitionCategoryItem leaderBoardItem)
	{
		CurrentRepository.AddCompetitionLeaderBoardItem(leaderBoardItem);
	}

	public void AddTournamentLeaderBoardItem(LeaderboardTournamentCategoryItem leaderboardTournamentCategoryItem)
	{
		CurrentRepository.AddTournamentLeaderBoardItem(leaderboardTournamentCategoryItem);
	}

	public List<TournamentCategoryLbCalculatedItem> CalculateTournamentCategoryLeaderBoard(TournamentId tournamentId, CategoryId categoryId, List<CompetitionId> tournamentCompetitionsId)
	{
		return CurrentRepository.CalculateTournamentCategoryLeaderBoard(tournamentId, categoryId, tournamentCompetitionsId);
	}

	public List<LeaderboardCompetitionCategoryItem> GetCompetitionCategoryLeaderBoard(CompetitionId competitionId, CategoryId categoryId)
	{
		return CurrentRepository.GetCompetitionCategoryLeaderBoard(competitionId, categoryId);
	}

	public IEnumerable<LeaderboardCompetitionCategoryItem> GetCompetitionLeaderBoard(CompetitionId competitionId)
	{
		return CurrentRepository.GetCompetitionLeaderBoard(competitionId);
	}

	public Dictionary<FisherId, List<(CompetitionId, int)>> GetFisherCompetitionPositions(List<CompetitionId> competitionsId, CategoryId categoryId)
	{
		return CurrentRepository.GetFisherCompetitionPositions(competitionsId, categoryId);
	}

	public List<LeaderboardTournamentCategoryItem> GetTournamentCategoryLeaderBoard(TournamentId tournamentId, CategoryId categoryId)
	{
		return CurrentRepository.GetTournamentCategoryLeaderBoard(tournamentId, categoryId);
	}

	public IEnumerable<LeaderboardTournamentCategoryItem> GetTournamentLeaderBoard(TournamentId tournamentIdValue)
	{
		return CurrentRepository.GetTournamentLeaderBoard(tournamentIdValue);
	}

	public void RemoveFisherFromLeaderboardCategory(TournamentId tournamentId, IEnumerable<CompetitionId> tournamentCompetitions, CategoryId categoryId, FisherId fisherId)
	{
		CurrentRepository.RemoveFisherFromLeaderboardCategory(tournamentId, tournamentCompetitions, categoryId, fisherId);
	}

	public void UpdateCompetitionLeaderBoardItem(LeaderboardCompetitionCategoryItem item)
	{
		CurrentRepository.UpdateCompetitionLeaderBoardItem(item);
	}

	public void UpdateTournamentLeaderBoardItem(LeaderboardTournamentCategoryItem item)
	{
		CurrentRepository.UpdateTournamentLeaderBoardItem(item);
	}
}

[CollectionDefinition(nameof(UseCaseTestsCollection))]
public class UseCaseTestsCollection : ICollectionFixture<UseCaseTestsFixture>
{
	// This class is never created.
}