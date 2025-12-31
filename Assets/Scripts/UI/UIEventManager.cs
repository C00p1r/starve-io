using System;

public static class UIEventManager
{
    // 定義一個接收字串的事件
    public static event Action<string, float> OnNotify;

    // 提供給各類腳本呼叫的靜態方法
    public static void TriggerNotify(string message, float fontSize = 70f)
    {
        OnNotify?.Invoke(message, fontSize);
    }
}
