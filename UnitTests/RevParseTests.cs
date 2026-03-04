using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sandbox.git;

[TestClass]
public class RevParseTests {
	[TestMethod]
	public void GetRepositoryTypeArgs_HappyPath_CommandLockedDown() {
		CollectionAssert.AreEqual(
			new[] { "rev-parse", "--is-bare-repository", "--show-cdup" },
			RevParse.GetRepositoryTypeArgs());
	}

	[TestMethod]
	public void GetUpstreamRefForRefArgs_HappyPath_CommandLockedDown() {
		CollectionAssert.AreEqual(
			new[] { "rev-parse", "--symbolic-full-name", "@{upstream}" },
			RevParse.GetUpstreamRefForRefArgs(null));
		CollectionAssert.AreEqual(
			new[] { "rev-parse", "--symbolic-full-name", "refs/heads/main@{upstream}" },
			RevParse.GetUpstreamRefForRefArgs("refs/heads/main"));
	}

	[TestMethod]
	public void GetCurrentBranchNameArgs_HappyPath_CommandLockedDown() {
		CollectionAssert.AreEqual(
			new[] { "rev-parse", "--abbrev-ref", "HEAD" },
			RevParse.GetCurrentBranchNameArgs());
	}

	[TestMethod]
	public async Task GetCurrentBranchNameAsync_ReturnsNullWhenPathIsEmpty() {
		var result = await RevParse.GetCurrentBranchNameAsync("");
		Assert.IsNull(result);
	}

	[TestMethod]
	public async Task GetCurrentBranchNameAsync_ReturnsNullWhenPathIsNull() {
		var result = await RevParse.GetCurrentBranchNameAsync(null);
		Assert.IsNull(result);
	}
}
