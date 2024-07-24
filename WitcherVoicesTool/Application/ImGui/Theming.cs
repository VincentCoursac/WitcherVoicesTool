using System.Numerics;
using ImGuiNET;

namespace WitcherVoicesTool.Application;

public class ThemeColor
{
    private readonly Vector4? _mainColor;
    private readonly Vector4? _hoveredColor;
    private readonly Vector4? _activeColor;
    private readonly Vector4? _disabledColor;
    private readonly Vector4? _contentColor;

    public Vector4 Main
    {
        get => _mainColor ?? Vector4.Zero;
        init => _mainColor = value;
    }

    public Vector4 Hovered
    {
        get => _hoveredColor ?? Main;
        init => _hoveredColor = value;
    }

    public Vector4 Active
    {
        get => _activeColor ?? Main;
        init => _activeColor = value;
    }

    public Vector4 Disabled
    {
        get => _disabledColor ?? Main;
        init => _disabledColor = value;
    }
    
    public Vector4 Content
    {
        get => _contentColor ?? new Vector4(0,0,0,1);
        init => _contentColor = value;
    }
}

public class Theming
{
    public static float TEXT_BASE_WIDTH = ImGui.CalcTextSize("A").X;
    
    public static readonly ThemeColor LightGreen = new ThemeColor
    {
        Main = new Vector4(0.212f, 0.988f, 0.565f, 1), 
        Content = new Vector4(0,0,0,1)
    };    
    
    public static readonly ThemeColor Yellow = new ThemeColor
    {
        Main = new Vector4(0.988f, 0.871f, 0.212f, 1),
        Hovered = new Vector4(1f, 0.682f, 0.208f, 1),
        Content = new Vector4(0,0,0,1)
    };
}