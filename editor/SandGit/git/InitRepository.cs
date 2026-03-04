using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sandbox.git;

/// <summary>
/// Initialize a new git repository.
/// </summary>
public static class InitRepository {
	const string OperationInitGitRepository = "initGitRepository";
	const string OperationGetDefaultBranch = "getDefaultBranch";

	static readonly IReadOnlySet<int> SuccessExitCodesConfig = new HashSet<int> { 0, 1 };

	/// <summary>
	/// Initializes a new git repository in the given path, using the configured or default initial branch name.
	/// </summary>
	public static async Task InitGitRepositoryAsync(string path) {
		var defaultBranch = await GetDefaultBranchAsync().ConfigureAwait(false);
		await Core.GitAsync(
			new[] { "-c", $"init.defaultBranch={defaultBranch}", "init" },
			path,
			OperationInitGitRepository
		).ConfigureAwait(false);
	}

	/// <summary>
	/// Returns the configured default branch for new repositories (e.g. from init.defaultBranch), or "main" if unset.
	/// </summary>
	public static async Task<string> GetDefaultBranchAsync() {
		var result = await Core.GitAsync(
			new[] { "config", "--global", "init.defaultBranch" },
			Environment.CurrentDirectory,
			OperationGetDefaultBranch,
			SuccessExitCodesConfig
		).ConfigureAwait(false);

		if ( result.ExitCode == 0 ) {
			var branch = result.Stdout.Trim();
			if ( !string.IsNullOrEmpty(branch) ) return branch;
		}

		return "main";
	}
}
