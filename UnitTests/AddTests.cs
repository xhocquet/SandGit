using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sandbox.SandGit.git;
using Sandbox.git.models;

[TestClass]
public class AddTests {
	[TestMethod]
	public void AddConflictedFileAsync_HappyPath_CommandLockedDownWithAllOptions() {
		var args = Add.GetAddConflictedFileArgs("path/to/resolved.txt");

		CollectionAssert.AreEqual(new[] { "add", "--", "path/to/resolved.txt" }, args);
	}

	[TestMethod]
	public async Task AddConflictedFileAsync_ThrowsWhenRepositoryIsNull() {
		var file = new GitWorkingDirectoryFileChange("file.txt", FileChangeKind.Modified);

		await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () =>
			await Add.AddConflictedFileAsync(null, file));
	}

	[TestMethod]
	public async Task AddConflictedFileAsync_ThrowsWhenFileIsNull() {
		var repository = new Repository("c:\\temp", 1, null, false);

		await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () =>
			await Add.AddConflictedFileAsync(repository, null));
	}

	[TestMethod]
	public async Task AddConflictedFileAsync_ThrowsWhenFilePathIsEmpty() {
		var repository = new Repository("c:\\temp", 1, null, false);
		var file = new GitWorkingDirectoryFileChange(string.Empty, FileChangeKind.Untracked);

		await Assert.ThrowsExceptionAsync<ArgumentException>(async () =>
			await Add.AddConflictedFileAsync(repository, file));
	}
}
