using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sandbox.git.models;
using BranchOps = Sandbox.git.Branch;

[TestClass]
public class BranchTests {
	[TestMethod]
	public void CreateBranchAsync_HappyPath_CommandLockedDownWithAllOptions() {
		CollectionAssert.AreEqual(
			new[] { "branch", "feature", "main" },
			BranchOps.GetCreateBranchArgs("feature", "main", false));
		CollectionAssert.AreEqual(
			new[] { "branch", "feature", "origin/main", "--no-track" },
			BranchOps.GetCreateBranchArgs("feature", "origin/main", true));
		CollectionAssert.AreEqual(
			new[] { "branch", "new-branch" },
			BranchOps.GetCreateBranchArgs("new-branch", null, false));
	}

	[TestMethod]
	public void GetBranchNamesArgs_HappyPath_CommandLockedDown() {
		CollectionAssert.AreEqual(
			new[] { "branch", "--format=%(refname:short)" },
			BranchOps.GetBranchNamesArgs());
	}

	[TestMethod]
	public void RenameBranchArgs_HappyPath_CommandLockedDown() {
		CollectionAssert.AreEqual(
			new[] { "branch", "-m", "old-name", "new-name" },
			BranchOps.GetRenameBranchArgs("old-name", "new-name", null));
		CollectionAssert.AreEqual(
			new[] { "branch", "-M", "old-name", "new-name" },
			BranchOps.GetRenameBranchArgs("old-name", "new-name", true));
	}

	[TestMethod]
	public void DeleteLocalBranchArgs_HappyPath_CommandLockedDown() {
		CollectionAssert.AreEqual(
			new[] { "branch", "-D", "stale-branch" },
			BranchOps.GetDeleteLocalBranchArgs("stale-branch"));
	}

	[TestMethod]
	public void DeleteRemoteBranchArgs_HappyPath_CommandLockedDown() {
		CollectionAssert.AreEqual(
			new[] { "push", "origin", ":feature" },
			BranchOps.GetDeleteRemoteBranchArgs("origin", "feature"));
	}

	[TestMethod]
	public void GetBranchesPointedAtArgs_HappyPath_CommandLockedDown() {
		CollectionAssert.AreEqual(
			new[] { "branch", "--points-at=abc123", "--format=%(refname:short)" },
			BranchOps.GetBranchesPointedAtArgs("abc123"));
	}

	[TestMethod]
	public void GetMergedBranchesArgs_HappyPath_CommandLockedDown() {
		CollectionAssert.AreEqual(
			new[] { "branch", "--format=%(objectname) %(refname)", "--merged", "main" },
			BranchOps.GetMergedBranchesArgs("main"));
	}

	[TestMethod]
	public void GetDeleteRefArgs_HappyPath_CommandLockedDown() {
		CollectionAssert.AreEqual(
			new[] { "update-ref", "-d", "refs/remotes/origin/gone" },
			BranchOps.GetDeleteRefArgs("refs/remotes/origin/gone"));
	}

	[TestMethod]
	public void FormatAsLocalRef_HappyPath() {
		Assert.AreEqual("refs/heads/main", BranchOps.FormatAsLocalRef("main"));
	}

	// --- Edge cases ---

	[TestMethod]
	public async Task CreateBranchAsync_ThrowsWhenRepositoryIsNull() {
		await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () =>
			await BranchOps.CreateBranchAsync(null, "branch", null));
	}

	[TestMethod]
	public async Task CreateBranchAsync_ThrowsWhenNameIsEmpty() {
		var repo = new Repository("c:\\temp", 1, null, false);
		await Assert.ThrowsExceptionAsync<ArgumentException>(async () =>
			await BranchOps.CreateBranchAsync(repo, "", null));
	}

	[TestMethod]
	public void GetCreateBranchArgs_ThrowsWhenNameIsEmpty() {
		Assert.ThrowsException<ArgumentException>(() =>
			BranchOps.GetCreateBranchArgs("", "main", false));
	}

	[TestMethod]
	public async Task GetBranchNamesAsync_ThrowsWhenRepositoryIsNull() {
		await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () =>
			await BranchOps.GetBranchNamesAsync(null));
	}

	[TestMethod]
	public async Task RenameBranchAsync_ThrowsWhenRepositoryIsNull() {
		var branch = new Sandbox.git.models.Branch("main", null, new BranchTip("abc"), BranchType.Local, "refs/heads/main");
		await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () =>
			await BranchOps.RenameBranchAsync(null, branch, "new-main"));
	}

	[TestMethod]
	public async Task RenameBranchAsync_ThrowsWhenBranchIsNull() {
		var repo = new Repository("c:\\temp", 1, null, false);
		await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () =>
			await BranchOps.RenameBranchAsync(repo, null, "new-main"));
	}

	[TestMethod]
	public async Task RenameBranchAsync_ThrowsWhenNewNameIsEmpty() {
		var repo = new Repository("c:\\temp", 1, null, false);
		var branch = new Sandbox.git.models.Branch("main", null, new BranchTip("abc"), BranchType.Local, "refs/heads/main");
		await Assert.ThrowsExceptionAsync<ArgumentException>(async () =>
			await BranchOps.RenameBranchAsync(repo, branch, ""));
	}

	[TestMethod]
	public void GetRenameBranchArgs_ThrowsWhenNewNameIsEmpty() {
		Assert.ThrowsException<ArgumentException>(() =>
			BranchOps.GetRenameBranchArgs("main", "", null));
	}

	[TestMethod]
	public async Task DeleteLocalBranchAsync_ThrowsWhenRepositoryIsNull() {
		await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () =>
			await BranchOps.DeleteLocalBranchAsync(null, "branch"));
	}

	[TestMethod]
	public async Task DeleteLocalBranchAsync_ThrowsWhenBranchNameIsEmpty() {
		var repo = new Repository("c:\\temp", 1, null, false);
		await Assert.ThrowsExceptionAsync<ArgumentException>(async () =>
			await BranchOps.DeleteLocalBranchAsync(repo, ""));
	}

	[TestMethod]
	public void GetDeleteLocalBranchArgs_ThrowsWhenBranchNameIsEmpty() {
		Assert.ThrowsException<ArgumentException>(() =>
			BranchOps.GetDeleteLocalBranchArgs(""));
	}

	sealed class FakeRemote : IRemote {
		public string Name { get; set; }
		public string Url { get; set; }
	}

	[TestMethod]
	public async Task DeleteRemoteBranchAsync_ThrowsWhenRepositoryIsNull() {
		var remote = new FakeRemote { Name = "origin", Url = "https://example.com/repo.git" };
		await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () =>
			await BranchOps.DeleteRemoteBranchAsync(null, remote, "feature"));
	}

	[TestMethod]
	public async Task DeleteRemoteBranchAsync_ThrowsWhenRemoteIsNull() {
		var repo = new Repository("c:\\temp", 1, null, false);
		await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () =>
			await BranchOps.DeleteRemoteBranchAsync(repo, null, "feature"));
	}

	[TestMethod]
	public async Task DeleteRemoteBranchAsync_ThrowsWhenRemoteBranchNameIsEmpty() {
		var repo = new Repository("c:\\temp", 1, null, false);
		var remote = new FakeRemote { Name = "origin", Url = "https://example.com/repo.git" };
		await Assert.ThrowsExceptionAsync<ArgumentException>(async () =>
			await BranchOps.DeleteRemoteBranchAsync(repo, remote, ""));
	}

	[TestMethod]
	public void GetDeleteRemoteBranchArgs_ThrowsWhenRemoteBranchNameIsEmpty() {
		Assert.ThrowsException<ArgumentException>(() =>
			BranchOps.GetDeleteRemoteBranchArgs("origin", ""));
	}

	[TestMethod]
	public async Task GetBranchesPointedAtAsync_ThrowsWhenRepositoryIsNull() {
		await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () =>
			await BranchOps.GetBranchesPointedAtAsync(null, "HEAD"));
	}

	[TestMethod]
	public async Task GetBranchesPointedAtAsync_ThrowsWhenCommitishIsEmpty() {
		var repo = new Repository("c:\\temp", 1, null, false);
		await Assert.ThrowsExceptionAsync<ArgumentException>(async () =>
			await BranchOps.GetBranchesPointedAtAsync(repo, ""));
	}

	[TestMethod]
	public void GetBranchesPointedAtArgs_ThrowsWhenCommitishIsEmpty() {
		Assert.ThrowsException<ArgumentException>(() =>
			BranchOps.GetBranchesPointedAtArgs(""));
	}

	[TestMethod]
	public async Task GetMergedBranchesAsync_ThrowsWhenRepositoryIsNull() {
		await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () =>
			await BranchOps.GetMergedBranchesAsync(null, "main"));
	}

	[TestMethod]
	public async Task GetMergedBranchesAsync_ThrowsWhenBranchNameIsEmpty() {
		var repo = new Repository("c:\\temp", 1, null, false);
		await Assert.ThrowsExceptionAsync<ArgumentException>(async () =>
			await BranchOps.GetMergedBranchesAsync(repo, ""));
	}

	[TestMethod]
	public void GetMergedBranchesArgs_ThrowsWhenBranchNameIsEmpty() {
		Assert.ThrowsException<ArgumentException>(() =>
			BranchOps.GetMergedBranchesArgs(""));
	}

	[TestMethod]
	public void FormatAsLocalRef_ThrowsWhenBranchNameIsEmpty() {
		Assert.ThrowsException<ArgumentException>(() =>
			BranchOps.FormatAsLocalRef(""));
	}

	[TestMethod]
	public void GetDeleteRefArgs_ThrowsWhenRefNameIsEmpty() {
		Assert.ThrowsException<ArgumentException>(() =>
			BranchOps.GetDeleteRefArgs(""));
	}
}
