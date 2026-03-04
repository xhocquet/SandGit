using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Sandbox.Diagnostics;

namespace Sandbox.git;

public readonly struct GitResult {
	public int ExitCode { get; }
	public string Stdout { get; }
	public string Stderr { get; }

	public GitResult(int exitCode, string stdout, string stderr) {
		ExitCode = exitCode;
		Stdout = stdout ?? string.Empty;
		Stderr = stderr ?? string.Empty;
	}
}

public static class Core {
	private static readonly Logger Logger = new Logger("SandGit[Git]");

	/// <summary>
	/// Runs a git operation with the given args, repository path, and operation name.
	/// </summary>
	public static void Git(string[] args, string path, string operationName) {
		_ = GitAsync(args, path, operationName).GetAwaiter().GetResult();
	}

	/// <summary>
	/// Runs a git operation asynchronously. Non-blocking.
	/// </summary>
	/// <param name="args">Git arguments (e.g. ["rev-parse", "--is-bare-repository"]).</param>
	/// <param name="path">Working directory (repository path).</param>
	/// <param name="operationName">Name used for logging.</param>
	/// <param name="successExitCodes">If set, exit codes in this set are treated as success; otherwise only 0 is success and other codes throw.</param>
	/// <param name="stdin">Optional input to send on stdin (e.g. null-separated paths for checkout-index --stdin -z).</param>
	/// <returns>Exit code, stdout, and stderr.</returns>
	public static async Task<GitResult> GitAsync(
		string[] args,
		string path,
		string operationName,
		IReadOnlySet<int> successExitCodes = null,
		string stdin = null
	) {
		using var process = new Process();
		process.StartInfo.FileName = "git";
		process.StartInfo.Arguments = BuildArguments(args);
		process.StartInfo.WorkingDirectory = path;
		process.StartInfo.RedirectStandardOutput = true;
		process.StartInfo.RedirectStandardError = true;
		process.StartInfo.UseShellExecute = false;
		process.StartInfo.CreateNoWindow = true;
		process.StartInfo.StandardOutputEncoding = Encoding.UTF8;
		process.StartInfo.StandardErrorEncoding = Encoding.UTF8;
		if ( stdin != null ) {
			process.StartInfo.RedirectStandardInput = true;
			process.StartInfo.StandardInputEncoding = Encoding.UTF8;
		}

		var gitCommand = path + ": git " + string.Join(" ", args);
		Logger.Trace(gitCommand);

		process.Start();
		if ( stdin != null ) {
			process.StandardInput.Write(stdin);
			process.StandardInput.Close();
		}

		var outTask = process.StandardOutput.ReadToEndAsync();
		var errTask = process.StandardError.ReadToEndAsync();
		await process.WaitForExitAsync().ConfigureAwait(false);
		var stdout = await outTask.ConfigureAwait(false);
		var stderr = await errTask.ConfigureAwait(false);

		Logger.Trace(gitCommand + " - " + process.ExitCode + " - " + stdout + " - " + stderr);

		var result = new GitResult(process.ExitCode, stdout, stderr);

		if ( successExitCodes != null && !successExitCodes.Contains(result.ExitCode) ) {
			throw new GitException(result, operationName);
		}

		if ( successExitCodes == null && result.ExitCode != 0 ) {
			throw new GitException(result, operationName);
		}

		return result;
	}

	static string BuildArguments(string[] args) {
		if ( args == null || args.Length == 0 ) return string.Empty;
		var sb = new StringBuilder();
		foreach ( var a in args ) {
			if ( sb.Length > 0 ) sb.Append(' ');
			if ( a.Contains(" ") ) {
				sb.Append('"').Append(a.Replace("\"", "\\\"")).Append('"');
			} else {
				sb.Append(a);
			}
		}

		return sb.ToString();
	}
}

public class GitException : Exception {
	public GitResult Result { get; }

	public GitException(GitResult result, string operationName)
		: base($"Git {operationName} failed with exit code {result.ExitCode}. stderr: {result.Stderr}") {
		Result = result;
	}
}
