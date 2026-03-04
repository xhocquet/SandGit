using Editor;
using Sandbox.git;
using Sandbox.widgets;
using System.Threading;
using System.Threading.Tasks;

namespace Sandbox;

[Dock("Editor", "SandGit", "account_tree")]
public class SandGitDock : Widget {
	const float OuterPadding = 4f;
	const float DockWidth = 300f;
	const int SandGitLoadDelay = 600;

	internal static SandGitDock Instance { get; private set; }

	readonly GitStore _gitStore;

	[Shortcut("sandgit.toggle-dock", "CTRL+G")]
	static void ToggleSandGitDock() {
		var currentlyOpen = EditorWindow.DockManager.IsDockOpen("SandGit");
		EditorWindow.DockManager.SetDockState("SandGit", !currentlyOpen);
		if ( !currentlyOpen ) {
			_ = Task.Delay(50).ContinueWith(_ => PlaceSandGitBelowHierarchy(),
				TaskScheduler.FromCurrentSynchronizationContext());
		}
	}

	[Shortcut("sandgit.refresh", "CTRL+R")]
	static void RefreshGitStore() {
		if ( Instance?._gitStore != null )
			Instance._gitStore.RequestDebouncedRefresh("shortcut");
	}

	/// <summary>
	/// When the editor starts, place SandGit on the left below Hierarchy if it is not already in the layout.
	/// </summary>
	[Event("editor.created")]
	static void OnEditorCreated(EditorMainWindow editorWindow) {
		_ = Task.Delay(SandGitLoadDelay).ContinueWith(_ => {
			if ( EditorWindow.DockManager.IsDockOpen("SandGit") )
				return;
			PlaceSandGitBelowHierarchy();
		}, TaskScheduler.FromCurrentSynchronizationContext());
	}

	public SandGitDock(Widget parent) : base(parent) {
		Instance = this;
		FixedWidth = DockWidth;
		MaximumWidth = DockWidth;
		MinimumHeight = 400f;
		Layout = Layout.Column();
		Layout.Spacing = 4f;

		var rootPath = Project.Current.GetRootPath();
		_gitStore = new GitStore(rootPath, SynchronizationContext.Current);
		EditorEvent.Register(_gitStore);

		var eventsManager = new EditorEventsManager();
		EditorEvent.Register(eventsManager);

		var topPad = new Widget(this) { FixedHeight = OuterPadding / 2 };
		var bottomPad = new Widget(this) { FixedHeight = OuterPadding / 2 };
		var leftPad = new Widget(this) { FixedWidth = OuterPadding };

		var contentWidth = DockWidth - 2f * OuterPadding;
		var content = new Widget(this) {
			Layout = Layout.Column(),
			FixedWidth = contentWidth,
			MaximumWidth = contentWidth,
			MinimumWidth = contentWidth
		};
		content.Layout.Spacing = 4f;
		content.Layout.Add(new RepositoryStatusWidget(content, _gitStore));
		content.Layout.Add(new BranchWidget(content, _gitStore));

		var changesWidget = new ChangesWidget(content, _gitStore);
		var historyWidget = new HistoryWidget(content, _gitStore);
		historyWidget.Visible = false;

		var pageSelect = new SegmentedControl(this);
		pageSelect.AddOption("Changes", "edit");
		pageSelect.AddOption("History", "history");
		pageSelect.OnSelectedChanged = _ => {
			changesWidget.Visible = pageSelect.SelectedIndex == 0;
			historyWidget.Visible = pageSelect.SelectedIndex == 1;
			if ( pageSelect.SelectedIndex == 1 )
				historyWidget.EnsureHistoryLoaded();
		};

		var tabRow = new Widget(this) { Layout = Layout.Row() };
		tabRow.Layout.Spacing = 8f;
		tabRow.Layout.Add(pageSelect);

		var tabContent = new Widget(this) { Layout = Layout.Column() };
		tabContent.Layout.Spacing = 4f;
		tabContent.Layout.Add(changesWidget, 1);
		tabContent.Layout.Add(historyWidget, 1);

		var tabbedSection = new Widget(this) { Layout = Layout.Column() };
		tabbedSection.Layout.Spacing = 4f;
		tabbedSection.Layout.Add(tabRow);
		tabbedSection.Layout.Add(tabContent, 1);

		content.Layout.Add(tabbedSection, 1);
		content.Layout.Add(new CommitWidget(content, _gitStore));

		var midRow = new Widget(this) { Layout = Layout.Row(), FixedWidth = DockWidth, MaximumWidth = DockWidth };
		midRow.Layout.Add(leftPad);
		midRow.Layout.Add(content);

		Layout.Add(topPad);
		Layout.Add(midRow, 1);
		Layout.Add(bottomPad);

		// Use debounced refresh so we don't run a second refresh (and second rev-parse) when an editor event fires shortly after open.
		_gitStore.RequestDebouncedRefresh("dock opened");
	}

	/// <summary>
	/// Try placing dock SandGit below Hierarchy.
	/// </summary>
	static bool PlaceSandGitBelowHierarchy() {
		var hierarchy = EditorWindow.DockManager.GetDockWidget("Hierarchy");
		if ( hierarchy == null || !hierarchy.IsValid )
			return false;

		var sandGit = EditorWindow.DockManager.GetDockWidget("SandGit");
		if ( sandGit == null || (sandGit is Widget w && !w.IsValid) )
			sandGit = EditorWindow.DockManager.Create<SandGitDock>();

		EditorWindow.DockManager.AddDock(hierarchy, sandGit, DockArea.Bottom);
		EditorWindow.DockManager.RaiseDock("SandGit");
		return true;
	}

	protected override void OnClosed() {
		if ( Instance == this )
			Instance = null;
		base.OnClosed();
	}
}
