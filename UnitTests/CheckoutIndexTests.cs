using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sandbox.git;
using Sandbox.git.models;

[TestClass]
public class CheckoutIndexTests {
	[TestMethod]
	public void GetCheckoutIndexArgs_HappyPath_CommandLockedDown() {
		CollectionAssert.AreEqual(
			new[] { "checkout-index", "-f", "-u", "-q", "--stdin", "-z" },
			CheckoutIndex.GetCheckoutIndexArgs());
	}

	[TestMethod]
	public void BuildCheckoutIndexStdin_HappyPath() {
		var paths = new[] { "file.txt", "dir/other.txt" };
		Assert.AreEqual("file.txt\0dir/other.txt", CheckoutIndex.BuildCheckoutIndexStdin(paths));
	}

	[TestMethod]
	public void BuildCheckoutIndexStdin_EmptyList_ReturnsEmptyString() {
		Assert.AreEqual("", CheckoutIndex.BuildCheckoutIndexStdin(new List<string>()));
	}

	[TestMethod]
	public void BuildCheckoutIndexStdin_Null_ReturnsEmptyString() {
		Assert.AreEqual("", CheckoutIndex.BuildCheckoutIndexStdin(null));
	}

	// --- Edge cases ---

	[TestMethod]
	public async Task CheckoutIndexAsync_ThrowsWhenRepositoryIsNull() {
		await Assert.ThrowsExceptionAsync<System.ArgumentNullException>(async () =>
			await CheckoutIndex.CheckoutIndexAsync(null, new[] { "file.txt" }));
	}

	[TestMethod]
	public async Task CheckoutIndexAsync_ThrowsWhenPathsIsNull() {
		var repo = new Repository("c:\\temp", 1, null, false);
		await Assert.ThrowsExceptionAsync<System.ArgumentNullException>(async () =>
			await CheckoutIndex.CheckoutIndexAsync(repo, null));
	}
}
