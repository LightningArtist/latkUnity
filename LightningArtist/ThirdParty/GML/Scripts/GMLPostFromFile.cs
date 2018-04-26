using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GMLPostFromFile : MonoBehaviour {

    public GMLDraw gmlDraw;
    public string url = "http://000000book.com/data";
    public string appName = "Unity";

    private void Awake() {
        if (gmlDraw == null) gmlDraw = GetComponent<GMLDraw>();
    }

    private void Start() {
		
	}
	
	private void Update() {
		if (Input.GetKeyDown(KeyCode.Space)) {
            StartCoroutine(postToBook());
        }
	}

    // https://github.com/jamiew/blackbook/wiki/Upload-GML-to-000000book
    // https://docs.unity3d.com/Manual/UnityWebRequest-SendingForm.html
	// https://docs.unity3d.com/ScriptReference/WWWForm.html
    /*
    keywords (string) [comma-separated list of keywords (‘tags’, not to be confused w/ graf tags)]
    location (string) [name like ‘NYC’, lat/long coordinates, or even a URL]
    username (string) [a 000000book user’s login]
    author (string) [the person who was actually writing]
    */

    private IEnumerator postToBook() {
        List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
        //formData.Add(new MultipartFormDataSection("field1=foo&field2=bar"));
        //formData.Add(new MultipartFormFileSection("my file data", "myfile.txt"));

		WWW www = new WWW("file://" + gmlDraw.url); 
		yield return www;

		string xmlString = www.text.Replace("\t","");//.Replace("\"", "");
		Debug.Log(xmlString);

		WWWForm form = new WWWForm();
		form.AddField("application", appName);
		form.AddField("gml", xmlString);

        www = new WWW(url, form);
		yield return www;

		if (!string.IsNullOrEmpty(www.error)) {
			print(www.error);
		} else {
            print("Finished posting GML to " + url);
		}

		/* 
		string fullString = "application=" + appName + "&gml=\"" + xmlString + "\"";
		formData.Add(new MultipartFormDataSection(fullString));
        UnityWebRequest wwwp = UnityWebRequest.Post(url, formData);
        yield return wwwp.Send();

        if (wwwp.isNetworkError) {
            Debug.Log(www.error);
        } else {
            Debug.Log("Form upload complete!");
        }
        */
    }

}
