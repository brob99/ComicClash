public static class MemeCreateState
{
    public static int SelectedDrawing = -1;
    public static string SelectedCaption = null;

    public static void Clear()
    {
        SelectedDrawing = -1;
        SelectedCaption = null;
    }
}
