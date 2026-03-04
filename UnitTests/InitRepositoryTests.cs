using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sandbox.git;

[TestClass]
public class InitRepositoryTests {
	[TestMethod]
	public void GetInitArgs_HappyPath_CommandLockedDown() {
		CollectionAssert.AreEqual(
			new[] { "-c", "init.defaultBranch=main", "init" },
			InitRepository.GetInitArgs("main"));
		CollectionAssert.AreEqual(
			new[] { "-c", "init.defaultBranch=master", "init" },
			InitRepository.GetInitArgs("master"));
	}

	[TestMethod]
	public void GetDefaultBranchConfigArgs_HappyPath_CommandLockedDown() {
		CollectionAssert.AreEqual(
			new[] { "config", "--global", "init.defaultBranch" },
			InitRepository.GetDefaultBranchConfigArgs());
	}

	// --- Edge cases ---

	[TestMethod]
	public void GetInitArgs_ThrowsWhenDefaultBranchIsEmpty() {
		Assert.ThrowsException<System.ArgumentException>(() =>
			InitRepository.GetInitArgs(""));
	}

	[TestMethod]
	public void GetInitArgs_ThrowsWhenDefaultBranchIsNull() {
		Assert.ThrowsException<System.ArgumentException>(() =>
			InitRepository.GetInitArgs(null));
	}
}
