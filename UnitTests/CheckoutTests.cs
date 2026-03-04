using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sandbox.git;
using Sandbox.git.models;

[TestClass]
public class CheckoutTests {
	[TestMethod]
	public void GetBranchCheckoutArgs_HappyPath_CommandLockedDown() {
		var localBranch = new Sandbox.git.models.Branch("main", null, new BranchTip("abc"), BranchType.Local, "refs/heads/main");
		CollectionAssert.AreEqual(
			new[] { "checkout", "main", "--" },
			Checkout.GetBranchCheckoutArgs(localBranch));

		var remoteBranch = new Sandbox.git.models.Branch("origin/feature", null, new BranchTip("def"), BranchType.Remote, "refs/remotes/origin/feature");
		CollectionAssert.AreEqual(
			new[] { "checkout", "-b", "feature", "origin/feature", "--" },
			Checkout.GetBranchCheckoutArgs(remoteBranch));
	}

	[TestMethod]
	public void GetCheckoutCommitArgs_HappyPath_CommandLockedDown() {
		CollectionAssert.AreEqual(
			new[] { "checkout", "abc123def" },
			Checkout.GetCheckoutCommitArgs("abc123def"));
	}

	[TestMethod]
	public void GetCheckoutPathsArgs_HappyPath_CommandLockedDown() {
		var paths = new[] { "file.txt", "dir/other.txt" };
		CollectionAssert.AreEqual(
			new[] { "checkout", "HEAD", "--", "file.txt", "dir/other.txt" },
			Checkout.GetCheckoutPathsArgs(paths));
	}

	[TestMethod]
	public void GetCheckoutConflictedFileArgs_HappyPath_CommandLockedDown() {
		CollectionAssert.AreEqual(
			new[] { "checkout", "--ours", "--", "path/conflict.txt" },
			Checkout.GetCheckoutConflictedFileArgs("path/conflict.txt", ManualConflictResolution.Ours));
		CollectionAssert.AreEqual(
			new[] { "checkout", "--theirs", "--", "path/conflict.txt" },
			Checkout.GetCheckoutConflictedFileArgs("path/conflict.txt", ManualConflictResolution.Theirs));
	}

	[TestMethod]
	public void GetCheckoutPathsArgs_EmptyList_ReturnsEmptyArray() {
		CollectionAssert.AreEqual(
			Array.Empty<string>(),
			Checkout.GetCheckoutPathsArgs(new List<string>()));
	}

	// --- Edge cases ---

	[TestMethod]
	public async Task CheckoutBranchAsync_ThrowsWhenRepositoryIsNull() {
		var branch = new Sandbox.git.models.Branch("main", null, new BranchTip("abc"), BranchType.Local, "refs/heads/main");
		await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () =>
			await Checkout.CheckoutBranchAsync(null, branch));
	}

	[TestMethod]
	public async Task CheckoutBranchAsync_ThrowsWhenBranchIsNull() {
		var repo = new Repository("c:\\temp", 1, null, false);
		await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () =>
			await Checkout.CheckoutBranchAsync(repo, null));
	}

	[TestMethod]
	public void GetBranchCheckoutArgs_ThrowsWhenBranchIsNull() {
		Assert.ThrowsException<ArgumentNullException>(() =>
			Checkout.GetBranchCheckoutArgs(null));
	}

	[TestMethod]
	public async Task CheckoutCommitAsync_ThrowsWhenRepositoryIsNull() {
		var commit = new CommitOneLine("abc123", "subject");
		await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () =>
			await Checkout.CheckoutCommitAsync(null, commit));
	}

	[TestMethod]
	public async Task CheckoutCommitAsync_ThrowsWhenCommitIsNull() {
		var repo = new Repository("c:\\temp", 1, null, false);
		await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () =>
			await Checkout.CheckoutCommitAsync(repo, null));
	}

	[TestMethod]
	public async Task CheckoutCommitAsync_ThrowsWhenShaIsEmpty() {
		var repo = new Repository("c:\\temp", 1, null, false);
		var commit = new CommitOneLine("", "subject");
		await Assert.ThrowsExceptionAsync<ArgumentException>(async () =>
			await Checkout.CheckoutCommitAsync(repo, commit));
	}

	[TestMethod]
	public void GetCheckoutCommitArgs_ThrowsWhenShaIsEmpty() {
		Assert.ThrowsException<ArgumentException>(() =>
			Checkout.GetCheckoutCommitArgs(""));
	}

	[TestMethod]
	public async Task CheckoutPathsAsync_ThrowsWhenRepositoryIsNull() {
		await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () =>
			await Checkout.CheckoutPathsAsync(null, new[] { "file.txt" }));
	}

	[TestMethod]
	public async Task CheckoutPathsAsync_ThrowsWhenPathsIsNull() {
		var repo = new Repository("c:\\temp", 1, null, false);
		await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () =>
			await Checkout.CheckoutPathsAsync(repo, null));
	}

	[TestMethod]
	public void GetCheckoutPathsArgs_ThrowsWhenPathsIsNull() {
		Assert.ThrowsException<ArgumentNullException>(() =>
			Checkout.GetCheckoutPathsArgs(null));
	}

	[TestMethod]
	public async Task CheckoutConflictedFileAsync_ThrowsWhenRepositoryIsNull() {
		var file = new GitWorkingDirectoryFileChange("file.txt", FileChangeKind.Modified);
		await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () =>
			await Checkout.CheckoutConflictedFileAsync(null, file, ManualConflictResolution.Ours));
	}

	[TestMethod]
	public async Task CheckoutConflictedFileAsync_ThrowsWhenFileIsNull() {
		var repo = new Repository("c:\\temp", 1, null, false);
		await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () =>
			await Checkout.CheckoutConflictedFileAsync(repo, null, ManualConflictResolution.Theirs));
	}

	[TestMethod]
	public async Task CheckoutConflictedFileAsync_ThrowsWhenFilePathIsEmpty() {
		var repo = new Repository("c:\\temp", 1, null, false);
		var file = new GitWorkingDirectoryFileChange("", FileChangeKind.Modified);
		await Assert.ThrowsExceptionAsync<ArgumentException>(async () =>
			await Checkout.CheckoutConflictedFileAsync(repo, file, ManualConflictResolution.Ours));
	}

	[TestMethod]
	public void GetCheckoutConflictedFileArgs_ThrowsWhenFilePathIsEmpty() {
		Assert.ThrowsException<ArgumentException>(() =>
			Checkout.GetCheckoutConflictedFileArgs("", ManualConflictResolution.Ours));
	}
}
