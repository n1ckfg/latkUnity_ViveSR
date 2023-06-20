using UnityEngine;

public class ColorReceiver_Custom : MonoBehaviour {

	public LightningArtist latk;
	public float minBrightness = 0.25f;
    Color color;

	void OnColorChange(HSBColor color) {
        this.color = color.ToColor();
		Color c = new Color(color.ToColor().r, color.ToColor().g, color.ToColor().b);
		if (c.r < minBrightness && c.g < minBrightness && c.b < minBrightness) {
			c.r += minBrightness;
			c.g += minBrightness;
			c.b += minBrightness;
		}
		latk.mainColor = new Color(color.ToColor().r, color.ToColor().g, color.ToColor().b);
	}

    /*
    void OnGUI() {
		var r = Camera.main.pixelRect;
		var rect = new Rect(r.center.x + r.height / 6 + 50, r.center.y, 100, 100);
		GUI.Label (rect, "#" + ToHex(color.r) + ToHex(color.g) + ToHex(color.b));	
    }

	string ToHex(float n) {
		return ((int)(n * 255)).ToString("X").PadLeft(2, '0');
	}
	*/

}
