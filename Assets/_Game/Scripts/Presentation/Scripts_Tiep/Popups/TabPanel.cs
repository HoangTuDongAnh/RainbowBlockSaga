using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Tab
{
    public string tabName;       // Tên hiển thị
    public Button tabButton;     // Button UI
    public GameObject tabContent; // Panel hiển thị khi active
}

public class TabPanel : MonoBehaviour
{
    [Header("Danh sách Tab")]
    public Tab[] tabs;
    public bool autoSelectFirstTab = true;

    [Header("Tùy chọn")]
    public Color activeColor = new Color(231f / 255f, 235f / 255f, 243f / 255f);
    public Color inactiveColor = new Color(198f / 255f, 201f / 255f, 214f / 255f);


    private void Start()
    {
        // Gán sự kiện click cho từng tab
        foreach (var tab in tabs)
        {
            var t = tab; // capture biến cho closure
            tab.tabButton.onClick.AddListener(() => ActivateTab(t));
        }

        // Mặc định kích hoạt tab đầu tiên
        if (autoSelectFirstTab && tabs.Length > 0)
            ActivateTab(tabs[0]);
    }

    public void ActivateTab(Tab tabToActivate)
    {
        foreach (var tab in tabs)
        {
            bool isActive = tab == tabToActivate;

            // Bật / tắt panel
            if (tab.tabContent != null)
                tab.tabContent.SetActive(isActive);

            // Thay đổi màu button để nổi bật
            if (tab.tabButton != null && tab.tabButton.image != null)
                tab.tabButton.image.color = isActive ? activeColor : inactiveColor;
            Debug.Log("Open tab:" + tabToActivate.tabName);
        }
       
    }
}
