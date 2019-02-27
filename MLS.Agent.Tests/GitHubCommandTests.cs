﻿using FluentAssertions;
using MLS.Repositories;
using System.Collections.Generic;
using System.CommandLine;
using System.Threading.Tasks;
using MLS.Agent.CommandLine;
using Xunit;
using static MLS.Agent.CommandLine.GitHubHandler;

namespace MLS.Agent.Tests
{
    public class GitHubCommandTests
    {
        private readonly IRepoLocator _locator = new RepoLocatorSimulator();

        class TestRepoLocator : IRepoLocator
        {
            private readonly IEnumerable<Repo> _repos;


            public TestRepoLocator(IEnumerable<Repo> repos)
            {
                _repos = repos;
            }

            public Task<IEnumerable<Repo>> LocateRepo(string repo)
            {
                return Task.FromResult(_repos);
            }
        }

        [Fact]
        public async Task It_reports_no_matches()
        {
            var console = new TestConsole();
            await Handler(new TryGitHubOptions("foo"), console, _locator);
            console.Out.ToString().Replace("\r\n", "\n")
                .Should().Be("Didn't find any repos called `foo`\n");

        }

        [Fact]
        public async Task It_finds_the_requested_repo()
        {
            var console = new TestConsole();
            await Handler(new TryGitHubOptions("rchande/2660eaec-6af8-452d-b70d-41227d616cd9"), console, _locator);
            console.Out.ToString().Replace("\r\n", "\n").
                Should().Be("Found repo `rchande/2660eaec-6af8-452d-b70d-41227d616cd9`\nTo try `rchande/2660eaec-6af8-452d-b70d-41227d616cd9`, cd to your desired directory and run the following command:\n\n\tgit clone https://github.com/rchande/2660eaec-6af8-452d-b70d-41227d616cd9.git && dotnet try .\n");

        }

        [Fact]
        public async Task It_asks_for_disambiguation()
        {
            var console = new TestConsole();
            await Handler(new TryGitHubOptions("rchande/tribble"), console, _locator);
            console.Out.ToString().Replace("\r\n", "\n").Should()
                .Be("Which of the following did you mean?\n\trchande/upgraded-octo-tribble.\n\trchande/downgraded-octo-tribble.\n");
        }
    }
}