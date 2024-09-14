using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class CategoryButtonManager : MonoBehaviour
{
    [SerializeField] private List<CategoryButton> categoryButtons;
    [SerializeField] private Importer importer;

    private void Start()
    {
        foreach (var button in categoryButtons)
        {
            if (button != null)
            {
                button.GetButton().onClick.AddListener(() => OnCategoryButtonClicked(button));
            }
            else
            {
                Debug.LogError("CategoryButton is null!");
            }
        }
    }

    private void OnCategoryButtonClicked(CategoryButton clickedButton)
    {
        foreach (var button in categoryButtons)
        {
            button.SetSelected(button == clickedButton);
        }

        string folderPath = GetFolderPathForCategory(clickedButton.name);
        importer.LoadThumbnailsFromFolder(folderPath);
    }

    private string GetFolderPathForCategory(string categoryName)
    {
        switch (categoryName)
        {
            case "Item":
                return "Assets/Files";
            case "Shelves":
                return "Assets/Shelves";
            default:
                return "Assets/Files";
        }
    }
}
