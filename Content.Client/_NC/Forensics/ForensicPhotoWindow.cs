using Content.Client._NC.CitiNet.UI;
using Content.Shared._NC.Forensics;
using Content.Client.UserInterface.Controls;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.CustomControls;
using Robust.Shared.Input;
using System.Numerics;

namespace Content.Client._NC.Forensics;

/// <summary>
/// Окно просмотра "фотоснимка" места преступления.
/// Показывает мини-карту вокруг точки смерти.
/// </summary>
public sealed class ForensicPhotoWindow : FancyWindow
{
    private readonly CitiNetMapControl _map;
    private readonly Label _victimLabel;
    private readonly Label _locationLabel;
    private readonly Label _timeLabel;
    private readonly Label _coordLabel;

    public ForensicPhotoWindow()
    {
        Title = "NCPD EVIDENCE REPORT & PHOTO";
        MinSize = new Vector2(500, 650);

        var root = new BoxContainer { Orientation = BoxContainer.LayoutOrientation.Vertical, Margin = new Thickness(15) };
        ContentsContainer.AddChild(root);

        // Header Section
        var header = new PanelContainer { Margin = new Thickness(0, 0, 0, 10), MinHeight = 100 };
        header.PanelOverride = new StyleBoxFlat { BackgroundColor = Color.FromHex("#0d1a1e"), BorderColor = Color.FromHex("#00E5FF"), BorderThickness = new Thickness(2, 2, 2, 0) };
        
        var headerContent = new BoxContainer { Orientation = BoxContainer.LayoutOrientation.Vertical, Margin = new Thickness(10) };
        headerContent.AddChild(new Label { Text = "NIGHT CITY POLICE DEPARTMENT", FontColorOverride = Color.FromHex("#00E5FF"), StyleClasses = { "LabelBig" } });
        headerContent.AddChild(new Label { Text = "FORENSICS DIVISION - BIO-CHIP DATA EXTRACT", FontColorOverride = Color.Gray });
        header.AddChild(headerContent);
        root.AddChild(header);

        // Data Section
        var dataPanel = new PanelContainer { Margin = new Thickness(0, 0, 0, 10) };
        dataPanel.PanelOverride = new StyleBoxFlat { BackgroundColor = Color.FromHex("#0d1a1e"), BorderColor = Color.FromHex("#00E5FF"), BorderThickness = new Thickness(2, 0, 2, 2) };
        
        var dataGrid = new GridContainer { Columns = 2, Margin = new Thickness(10) };
        
        dataGrid.AddChild(new Label { Text = "SUBJECT:", FontColorOverride = Color.Gray });
        _victimLabel = new Label { FontColorOverride = Color.Yellow };
        dataGrid.AddChild(_victimLabel);

        dataGrid.AddChild(new Label { Text = "LOCATION:", FontColorOverride = Color.Gray });
        _locationLabel = new Label();
        dataGrid.AddChild(_locationLabel);

        dataGrid.AddChild(new Label { Text = "TIMESTAMP:", FontColorOverride = Color.Gray });
        _timeLabel = new Label { FontColorOverride = Color.LightSkyBlue };
        dataGrid.AddChild(_timeLabel);

        dataGrid.AddChild(new Label { Text = "COORDINATES:", FontColorOverride = Color.Gray });
        _coordLabel = new Label { FontColorOverride = Color.LightSkyBlue };
        dataGrid.AddChild(_coordLabel);

        dataPanel.AddChild(dataGrid);
        root.AddChild(dataPanel);

        // Photo (Map) Section
        root.AddChild(new Label { Text = "ATTACHED VISUAL DATA:", Margin = new Thickness(0, 5) });
        
        var mapPanel = new PanelContainer { VerticalExpand = true, RectClipContent = true };
        mapPanel.PanelOverride = new StyleBoxFlat { BackgroundColor = Color.Black, BorderColor = Color.Red, BorderThickness = new Thickness(2) };
        
        _map = new CitiNetMapControl { HorizontalExpand = true, VerticalExpand = true };
        mapPanel.AddChild(_map);
        root.AddChild(mapPanel);

        root.AddChild(new Label { Text = "CONFIDENTIAL - NCPD PROPERTY", HorizontalAlignment = HAlignment.Center, FontColorOverride = Color.FromHex("#444444"), Margin = new Thickness(0, 5, 0, 0) });
    }

    public void UpdateData(ForensicPhotoComponent photo)
    {
        _victimLabel.Text = photo.VictimName.ToUpper();
        _locationLabel.Text = photo.LocationName;
        _timeLabel.Text = photo.Timestamp.ToString(@"hh\:mm\:ss");
        _coordLabel.Text = $"{photo.Coordinates.X:0.0}, {photo.Coordinates.Y:0.0}";

        var entManager = IoCManager.Resolve<IEntityManager>();
        var coords = entManager.GetCoordinates(photo.Coordinates);

        if (coords.EntityId.Valid)
        {
            _map.MapUid = coords.EntityId;
            _map.ForceNavMapUpdate();
            _map.CenterToCoordinates(coords);
            _map.MapRange = 25f; // Good zoom for a "photo"
            
            _map.MapBeacons.Clear();
            _map.MapBeacons.Add(new Content.Shared._NC.CitiNet.CitiNetMapBeaconData(
                netEnt: NetEntity.Invalid,
                label: "BODY",
                icon: null,
                color: Color.Red,
                localPosition: coords.Position,
                fontSize: 12
            ) { IsDead = true });
        }
    }
}
