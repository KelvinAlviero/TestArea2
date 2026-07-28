using UnityEngine;


[CreateAssetMenu(fileName = "Powerup Data", menuName = "Powerups")]
public class Powerups : ScriptableObject
{
    public Sprite PowerupImage;
    public string package_id;
    // public List <string> items;
    public string item_id;
	public string powerupName;
    public string item_type;
    public int amount;
}
