using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sandbox.git;
using Sandbox.git.models;

[TestClass]
public class ForEachRefTests {
	[TestMethod]
	public void GetForEachRefArgs_HappyPath_CommandLockedDown() {
		var format = "%(refname)%00%(refname:short)%00%(upstream:short)%00%(objectname)%00%(symref)";
		var prefixes = new[] { "refs/heads", "refs/remotes" };
		CollectionAssert.AreEqual(
			new[] { "for-each-ref", "--format=" + format, "refs/heads", "refs/remotes" },
			ForEachRef.GetForEachRefArgs(format, prefixes));
	}

	[TestMethod]
	public void GetForEachRefArgs_SinglePrefix_CommandLockedDown() {
		CollectionAssert.AreEqual(
			new[] { "for-each-ref", "--format=%(refname:short)", "refs/heads" },
			ForEachRef.GetForEachRefArgs("%(refname:short)", new[] { "refs/heads" }));
	}

	// --- Edge cases ---

	[TestMethod]
	public async Task GetBranchesAsync_ThrowsWhenRepositoryIsNull() {
		await Assert.ThrowsExceptionAsync<System.ArgumentNullException>(async () =>
			await ForEachRef.GetBranchesAsync(null));
	}

	[TestMethod]
	public async Task GetBranchesDifferingFromUpstreamAsync_ThrowsWhenRepositoryIsNull() {
		await Assert.ThrowsExceptionAsync<System.ArgumentNullException>(async () =>
			await ForEachRef.GetBranchesDifferingFromUpstreamAsync(null));
	}

	[TestMethod]
	public void GetForEachRefArgs_ThrowsWhenFormatIsNull() {
		Assert.ThrowsException<System.ArgumentNullException>(() =>
			ForEachRef.GetForEachRefArgs(null, new[] { "refs/heads" }));
	}

	[TestMethod]
	public void GetForEachRefArgs_ThrowsWhenPrefixesIsNull() {
		Assert.ThrowsException<System.ArgumentException>(() =>
			ForEachRef.GetForEachRefArgs("%(refname)", null));
	}

	[TestMethod]
	public void GetForEachRefArgs_ThrowsWhenPrefixesIsEmpty() {
		Assert.ThrowsException<System.ArgumentException>(() =>
			ForEachRef.GetForEachRefArgs("%(refname)", new string[0]));
	}
}
