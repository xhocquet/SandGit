#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sandbox.git.models;

namespace Sandbox.git;

/// <summary>
/// Checkout operations: branch, commit, paths at HEAD, and conflicted file resolution.
/// Mirrors Desktop's checkout.ts (branch, commit, paths, conflicted file).
/// </summary>
public static class Checkout {
	const string OperationCheckoutBranch = "checkoutBranch";
	const string OperationCheckoutCommit = "checkoutCommit";
	const string OperationCheckoutPaths = "checkoutPaths";
	const string OperationCheckoutConflictedFile = "checkoutConflictedFile";

	// ─── Public API ─────────────────────────────────────────────────────────

	/// <summary>
	/// Check out the given branch. For a remote branch, creates a local branch with the same short name (-b).
	/// No-op if already on the given local branch.
	/// </summary>
	/// <param name="repository">The repository in which to perform the checkout.</param>
	/// <param name="branch">The branch to check out (local or remote).</param>
	public static async Task CheckoutBranchAsync(Repository repository, models.Branch branch) {
		if ( repository == null )
			throw new ArgumentNullException(nameof(repository));
		if ( branch == null )
			throw new ArgumentNullException(nameof(branch));

		var args = GetBranchCheckoutArgs(branch);
		await Core.GitAsync(args, repository.Path, OperationCheckoutBranch).ConfigureAwait(false);
	}

	/// <summary>
	/// Check out the given commit (detached HEAD). Literally runs git checkout &lt;sha&gt;.
	/// </summary>
	/// <param name="repository">The repository in which to perform the checkout.</param>
	/// <param name="commit">The commit to check out (use commit.Sha).</param>
	public static async Task CheckoutCommitAsync(Repository repository, CommitOneLine commit) {
		if ( repository == null )
			throw new ArgumentNullException(nameof(repository));
		if ( commit == null )
			throw new ArgumentNullException(nameof(commit));
		if ( string.IsNullOrEmpty(commit.Sha) )
			throw new ArgumentException("Commit SHA is required.", nameof(commit));

		var args = new[] { "checkout", commit.Sha };
		await Core.GitAsync(args, repository.Path, OperationCheckoutCommit).ConfigureAwait(false);
	}

	/// <summary>
	/// Check out the given paths at HEAD. Reverts working tree (and index) for those paths to HEAD.
	/// </summary>
	/// <param name="repository">The repository.</param>
	/// <param name="paths">Paths to revert (relative to repo root). Empty list is a no-op.</param>
	public static async Task CheckoutPathsAsync(Repository repository, IReadOnlyList<string> paths) {
		if ( repository == null )
			throw new ArgumentNullException(nameof(repository));
		if ( paths == null )
			throw new ArgumentNullException(nameof(paths));

		if ( paths.Count == 0 )
			return;

		var args = new[] { "checkout", "HEAD", "--" }
			.Concat(paths)
			.ToArray();
		await Core.GitAsync(args, repository.Path, OperationCheckoutPaths).ConfigureAwait(false);
	}

	/// <summary>
	/// Check out either "ours" (stage #2) or "theirs" (stage #3) for a conflicted file.
	/// </summary>
	/// <param name="repository">The repository.</param>
	/// <param name="file">The conflicted file (path).</param>
	/// <param name="resolution">Ours or theirs.</param>
	public static async Task CheckoutConflictedFileAsync(
		Repository repository,
		GitWorkingDirectoryFileChange file,
		ManualConflictResolution resolution) {
		if ( repository == null )
			throw new ArgumentNullException(nameof(repository));
		if ( file == null )
			throw new ArgumentNullException(nameof(file));
		if ( string.IsNullOrEmpty(file.Path) )
			throw new ArgumentException("File path is required.", nameof(file));

		var resolutionFlag = resolution == ManualConflictResolution.Ours ? "--ours" : "--theirs";
		var args = new[] { "checkout", resolutionFlag, "--", file.Path };
		await Core.GitAsync(args, repository.Path, OperationCheckoutConflictedFile).ConfigureAwait(false);
	}

	// ─── Private helpers ────────────────────────────────────────────────────

	static string[] GetBranchCheckoutArgs(models.Branch branch) {
		if ( branch.Type == BranchType.Remote ) {
			// Create local branch from remote: checkout -b <nameWithoutRemote> <name> --
			return new[] { "checkout", "-b", branch.NameWithoutRemote, branch.Name, "--" };
		}

		// Local branch: checkout <name> --
		return new[] { "checkout", branch.Name, "--" };
	}
}
