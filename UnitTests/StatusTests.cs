using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sandbox.git;
using Sandbox.git.models;

[TestClass]
public class StatusTests {
	[TestMethod]
	public void GetStatusArgs_HappyPath_CommandLockedDown() {
		CollectionAssert.AreEqual(
			new[] { "status", "--branch", "--porcelain=2" },
			Status.GetStatusArgs());
	}

	[TestMethod]
	public void GetFullStatusArgs_HappyPath_CommandLockedDown() {
		CollectionAssert.AreEqual(
			new[] { "--no-optional-locks", "status", "--untracked-files=all", "--branch", "--porcelain=2", "-z" },
			Status.GetFullStatusArgs());
	}

	[TestMethod]
	public async Task GetStatusAsync_ThrowsWhenRepositoryIsNull() {
		await Assert.ThrowsExceptionAsync<System.ArgumentNullException>(async () =>
			await Status.GetStatusAsync((Repository)null));
	}

	[TestMethod]
	public async Task GetStatusAsync_ReturnsNullWhenPathIsEmpty() {
		var result = await Status.GetStatusAsync("");
		Assert.IsNull(result);
	}

	[TestMethod]
	public async Task GetStatusAsync_ReturnsNullWhenPathIsNull() {
		var result = await Status.GetStatusAsync((string)null);
		Assert.IsNull(result);
	}

	[TestMethod]
	public async Task GetFullStatusAsync_ReturnsNullWhenPathIsEmpty() {
		var result = await Status.GetFullStatusAsync("");
		Assert.IsNull(result);
	}

	[TestMethod]
	public async Task GetFullStatusAsync_ReturnsNullWhenPathIsNull() {
		var result = await Status.GetFullStatusAsync(null);
		Assert.IsNull(result);
	}
}
