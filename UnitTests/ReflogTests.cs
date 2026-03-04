using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sandbox.git;

[TestClass]
public class ReflogTests {
	[TestMethod]
	public void GetRecentBranchesArgs_HappyPath_CommandLockedDown() {
		CollectionAssert.AreEqual(
			new[] { "log", "-g", "--no-abbrev-commit", "--pretty=oneline", "HEAD", "-n", "2500", "--" },
			Reflog.GetRecentBranchesArgs());
	}

	[TestMethod]
	public void GetBranchCheckoutsArgs_HappyPath_CommandLockedDown() {
		var after = new DateTime(2025, 3, 1, 12, 0, 0, DateTimeKind.Utc);
		var args = Reflog.GetBranchCheckoutsArgs(after);
		Assert.AreEqual(6, args.Length);
		Assert.AreEqual("reflog", args[0]);
		Assert.AreEqual("--date=iso", args[1]);
		Assert.IsTrue(args[2].StartsWith("--after=\""));
		Assert.AreEqual("--pretty=%H %gd %gs", args[3]);
		Assert.AreEqual("--grep-reflog=checkout: moving from .* to .*$", args[4]);
		Assert.AreEqual("--", args[5]);
	}

	[TestMethod]
	public async Task GetRecentBranchesAsync_ReturnsEmptyWhenRepositoryIsNull() {
		var result = await Reflog.GetRecentBranchesAsync(null, 5);
		Assert.IsNotNull(result);
		Assert.AreEqual(0, result.Count);
	}

	[TestMethod]
	public async Task GetBranchCheckoutsAsync_ReturnsEmptyWhenRepositoryIsNull() {
		var result = await Reflog.GetBranchCheckoutsAsync(null, DateTime.UtcNow);
		Assert.IsNotNull(result);
		Assert.AreEqual(0, result.Count);
	}
}
