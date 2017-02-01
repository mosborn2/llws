using UnityEngine;
using System.Collections;

public class TimerBox : MonoBehaviour
{

	bool state = false;
	public UnityEngine.UI.Text tBox;
  	public Overlord o;
	private float StartTime;
	private float textHeight;

  void Update()
  {
      if (o != null){ 
		int rawSeconds = (int)(int)(o.endTime - Time.time);
		int minutes = rawSeconds / 60;
		int seconds = rawSeconds % 60;
      	tBox.text = string.Format("{0,2:D2}:{1,2:D2}",minutes, seconds);
      	
      	if(o.endTime != 0)
			tBox.transform.position = new Vector2(((Time.time - StartTime) / (o.endTime - StartTime)) * Screen.width, textHeight);
      }
  }

  void Start()
  {
    tBox.text = "Ready?!";
    //o = GameObject.FindGameObjectWithTag("master");
//    StartCoroutine(updateTimer());
	StartTime = Time.time;
	textHeight = this.transform.position.y;
  }

  
}
