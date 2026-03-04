using System;
using System.IO;
using Editor;
using Sandbox.Diagnostics;
using Sandbox.git;
using Sandbox.git.models;

namespace Sandbox.widgets;

public class RepositoryStatusWidget : Widget {
	const float StatusButtonSize = 28f;

	private readonly GitStore _store;
	private readonly ConnectionStatusButton _statusButton;
	private readonly RepoNameField _repoNameField;
	private readonly RepoDropdownIndicator _dropdownIndicator;
	private static readonly Logger Logger = new Logger("SandGit[RepoWidget]");

	public RepositoryStatusWidget(Widget parent, GitStore store) : base(parent) {
		_store = store ?? throw new ArgumentNullException(nameof(store));

		Layout = Layout.Column();
		Layout.Spacing = 6f;

		var row = new Widget(this) { Layout = Layout.Row() };
		row.Layout.Spacing = 4f;

		_statusButton = new ConnectionStatusButton(row, _store) {
			FixedWidth = StatusButtonSize, FixedHeight = StatusButtonSize
		};
		_repoNameField = new RepoNameField(row);
		_dropdownIndicator = new RepoDropdownIndicator(row) { Icon = null };
		_dropdownIndicator.Clicked += OnDropdownClicked;

		row.Layout.Add(_statusButton);
		row.Layout.Add(_repoNameField, 1);
		row.Layout.Add(_dropdownIndicator);
		Layout.Add(row);

		_store.OnDataChanged += UpdateFromStore;
		UpdateFromStore();
	}

	protected override void OnClosed() {
		_store.OnDataChanged -= UpdateFromStore;
		base.OnClosed();
	}

	void OnDropdownClicked() {
		var menu = new ContextMenu(null);
		if ( _store.RepositoryType is MissingRepositoryType )
			menu.AddOption("Create Repo", "add", OnCreateRepoClicked);
		menu.AddOption("Refresh", "refresh", () => _store.RequestDebouncedRefresh("menu refresh"));
		menu.OpenAtCursor();
	}

	async void OnCreateRepoClicked() {
		var rootPath = _store.RootPath;
		if ( string.IsNullOrEmpty(rootPath) ) return;

		_repoNameField.Text = "Creating repository…";

		var fullPath = rootPath;
		try {
			Directory.CreateDirectory(fullPath);
		} catch ( UnauthorizedAccessException ex ) {
			Logger.Trace($"Create repo: access denied at {fullPath}: {ex.Message}");
			ShowCreateError("You may not have permission to create a directory here.");
			return;
		} catch ( Exception ex ) when
			( ex is ArgumentException or PathTooLongException or DirectoryNotFoundException ) {
			Logger.Trace($"Create repo: invalid path {fullPath}: {ex.Message}");
			ShowCreateError("The path is invalid or could not be created.");
			return;
		}

		try {
			await InitRepository.InitGitRepositoryAsync(fullPath).ConfigureAwait(true);
		} catch ( GitException ex ) {
			Logger.Trace($"Create repo: init failed at {fullPath}: {ex.Message}");
			ShowCreateError($"Git init failed: {ex.Result.Stderr.Trim()}");
			return;
		}

		_store.RequestDebouncedRefresh("create repo");
	}

	void ShowCreateError(string message) {
		_repoNameField.Text = message;
	}

	void UpdateFromStore() {
		if ( !IsValid )
			return;
		if ( _repoNameField == null )
			return;
		Update();
		if ( _statusButton != null ) {
			_statusButton.ToolTip = BuildConnectionToolTip();
			_statusButton.Update();
		}

		if ( _store.IsLoading ) {
			_repoNameField.Text = "Checking for repository…";
			return;
		}

		var repoType = _store.RepositoryType;
		if ( repoType == null ) {
			_repoNameField.Text = "Checking for repository…";
			return;
		}

		if ( repoType is MissingRepositoryType ) {
			_repoNameField.Text = "No repository found";
			return;
		}

		_repoNameField.Text = GetDisplayRepoName(repoType);
	}

	string BuildConnectionToolTip() {
		var repoType = _store.RepositoryType;
		var path = repoType != null ? GetDisplayPath(repoType) : null;
		if ( string.IsNullOrEmpty(path) || path == "(bare)" )
			path = _store.RootPath;
		if ( string.IsNullOrEmpty(path) )
			path = "(none)";
		var status = _store.IsLoading ? "pending" : (repoType is MissingRepositoryType ? "missing" : "active");
		return $"{path}\nConnection {status}";
	}

	static string GetDisplayRepoName(RepositoryType repoType) {
		var fullPath = GetDisplayPath(repoType);
		if ( string.IsNullOrEmpty(fullPath) ) return string.Empty;
		if ( fullPath == "(bare)" ) return fullPath;
		return GetRepoNameFromPath(fullPath);
	}

	static string GetRepoNameFromPath(string repoPath) {
		if ( string.IsNullOrEmpty(repoPath) ) return string.Empty;
		var trimmed = repoPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
		var name = Path.GetFileName(trimmed);
		return string.IsNullOrEmpty(name) ? repoPath : name;
	}

	static string GetDisplayPath(RepositoryType repoType) {
		if ( repoType is RegularRepositoryType regular )
			return regular.TopLevelWorkingDirectory;
		if ( repoType is UnsafeRepositoryType unsafeRepo )
			return unsafeRepo.RepositoryPath;
		if ( repoType is BareRepositoryType )
			return "(bare)";
		return string.Empty;
	}
}

sealed class ConnectionStatusButton : Widget {
	readonly GitStore _store;
	const int PowerIconSize = 14;

	public ConnectionStatusButton(Widget parent, GitStore store) : base(parent) {
		_store = store ?? throw new ArgumentNullException(nameof(store));
		MinimumSize = 24;
	}

	protected override void OnPaint() {
		var r = new Rect(0, Size);
		var repoType = _store.RepositoryType;
		var iconColor = Theme.Yellow;
		if ( repoType is MissingRepositoryType ) {
			iconColor = Theme.Red;
		} else {
			iconColor = Theme.Green;
		}

		var bgColor = iconColor.Darken(0.7f).Desaturate(0.5f);

		Paint.ClearPen();
		Paint.SetBrush(in bgColor);
		Paint.DrawRect(r, 4f);

		var iconRect = WidgetPaintUtils.GetCenteredIconRect(r, PowerIconSize, 6, 6, 6, 6);

		Paint.SetPen(in iconColor);
		Paint.SetBrush(in iconColor);
		Paint.DrawIcon(iconRect, "power", PowerIconSize);
	}
}

sealed class RepoDropdownIndicator : Button {
	public RepoDropdownIndicator(Widget parent) : base(parent) {
		FixedWidth = 12f;
		MinimumHeight = 24f;
		ToolTip = "Repository options";
	}

	protected override void OnPaint() {
		WidgetPaintUtils.DrawDropdownChevron(new Rect(0, Size));
	}
}

sealed class RepoNameField : Widget {
	string _text = "";
	const float Padding = 8f;

	public string Text {
		get => _text;
		set {
			if ( _text == value ) return;
			_text = value ?? "";
			Update();
		}
	}

	public RepoNameField(Widget parent) : base(parent) {
		MinimumHeight = 28f;
		MinimumWidth = 0;
	}

	protected override void OnPaint() {
		var r = new Rect(0, Size);
		WidgetPaintUtils.DrawTextFieldBackground(r);

		var textRect = r.Shrink(Padding, 4f, Padding, 4f);
		if ( string.IsNullOrEmpty(_text) )
			return;
		Paint.SetDefaultFont();
		Paint.SetPen(Theme.Text);
		Paint.DrawText(textRect, _text, TextFlag.LeftCenter);
	}
}
