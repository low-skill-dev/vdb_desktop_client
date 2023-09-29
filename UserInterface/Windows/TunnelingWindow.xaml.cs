using ApiModels.Node;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace UserInterface.Windows;

public partial class TunnelingWindow : Window
{
	private readonly CancellationTokenSource _cancellationTokenSource;
	private readonly AuthWindow _parent;
	private readonly StateHelper _stateHelper;
	private DateTime _lastReceivedServers;

	public TunnelingWindow(AuthWindow parent)
	{
		_cancellationTokenSource = new();
		_parent = parent;
		_stateHelper = new();

		InitializeComponent();
	}

	protected override void OnInitialized(EventArgs e)
	{
		Task.Run(this.LoadNodes);

		base.OnInitialized(e);
	}

	private void SetDisplayedServersList(IEnumerable<PublicNodeInfo>? nodes, int? active = null)
	{
		if(nodes is null || !nodes.Any()) return;

		foreach(var node in nodes)
		{
			// created border element
			var border = new Border()
			{
				BorderThickness = new(2),
				CornerRadius = new(2 * 3),
				BorderBrush = new SolidColorBrush(Colors.Black),
				Margin = new(0, 5, 0, 0)
			};

			// create text block as button
			var additionalSpace = node.Id.ToString().Length > 1 ? "" : " ";
			var block = new TextBlock()
			{
				// https://www.autoitscript.com/autoit3/docs/appendix/fonts.htm
				Text = $"#{node.Id}{additionalSpace} {node.Name}",
				FontFamily = new System.Windows.Media.FontFamily(@"Lucida Console"),
				FontWeight = FontWeight.FromOpenTypeWeight(600),
				Padding = new(5),
				Name = node.Id.ToString()
			};

			// create event listener on double click
			block.MouseDown += async (_, e) =>
			{
				if(e.ClickCount == 2)
				{
					try
					{
						this.WrapperGrid.IsEnabled = false;
						await this.ConnectToSelectedNode(node.Id);
					}
					finally
					{
						this.WrapperGrid.IsEnabled = true;
					}
				}
			};

			border.Child = block;
			this.ServersListSP.Children.Add(border);
		}
	}

	private void ChangeDisplayedActiveNode(int? nodeId)
	{
		for(int i = 0; i < this.ServersListSP.Children.Count; i++)
		{
			var nodeComponent = (TextBlock)((Border)this.ServersListSP.Children[i]).Child;

			// parse id's from existing buttons
			var node = GetNodeIdFromItsTextBlock(nodeComponent);

			nodeComponent.Background = new SolidColorBrush(
				nodeId.HasValue && node == nodeId.Value
				? Colors.LightGreen : Colors.White);
		}
	}

	private int GetNodeIdFromItsTextBlock(TextBlock b) => int.Parse(b.Name);



	private async Task ConnectToSelectedNode(int nodeId)
	{

	}

	private async Task LoadNodes()
	{
		await _stateHelper.LoadNodes();

		SetDisplayedServersList(_stateHelper.Nodes);
	}

	private async Task StartBackgroundTask(CancellationToken ct)
	{
		while(!ct.IsCancellationRequested)
		{
			await Task.Delay(TimeSpan.FromHours(1));

			await _stateHelper.LoadNodes();

			SetDisplayedServersList(_stateHelper.Nodes);
			ChangeDisplayedActiveNode(_stateHelper.CurrentlyConnectedNode);
		}
	}
}
