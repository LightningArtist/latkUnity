using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class LatkToGML : MonoBehaviour { 

    public enum PostMode { KEYBOARD, AUTO, NONE };
    public PostMode postMode = PostMode.AUTO;
    public float autoPostInterval = 5f;

    public LightningArtist latk;
    public string url = "http://000000book.com/data";
    public string appName = "Unity";
    public Vector3 dim = new Vector3(480f, 320f, 18f); // Fat Tag default
    public float timeIncrement = 0.01f;

    private void Start() {
        if (postMode == PostMode.AUTO) {
            StartCoroutine(autoPoster());
        }
    }

    private void Update() {
        if (postMode == PostMode.KEYBOARD) {
            if (Input.GetKeyUp(KeyCode.Space)) {
                doPost();
            }
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

    private IEnumerator autoPoster() {
        while (true) {
            yield return new WaitForSeconds(autoPostInterval);
            if (latk.layerList[latk.currentLayer].frameList[latk.layerList[latk.currentLayer].currentFrame].brushStrokeList.Count > 1) doPost();
        }
    }

    public void doPost() {
        StartCoroutine(postToBook());
    }

    private IEnumerator postToBook() {
        List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
        //formData.Add(new MultipartFormDataSection("field1=foo&field2=bar"));
        //formData.Add(new MultipartFormFileSection("my file data", "myfile.txt"));

        /*
        WWW www = new WWW("file://" + gmlDraw.url); 
		yield return www;

		string xmlString = www.text.Replace("\t","");//.Replace("\"", "");
        */
        List<LatkStroke> strokes = latk.layerList[latk.currentLayer].frameList[latk.layerList[latk.currentLayer].currentFrame].brushStrokeList;
        //Debug.Log("!!!" + strokes.Count);
        string xmlString = buildGml(strokes, dim).Replace("\t", "");
        Debug.Log(xmlString);

		WWWForm form = new WWWForm();
		form.AddField("application", appName);
		form.AddField("gml", xmlString);

        WWW www = new WWW(url, form);
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

    private float timeCounter = 0f;
    private Vector3 maxPoint = Vector3.zero;
    private Vector3 minPoint = Vector3.zero;

    string buildGml(List<LatkStroke> strokes, Vector3 _dim) {
        timeCounter = 0f;
        maxPoint = Vector3.zero;
        minPoint = Vector3.zero;
        string returns = gmlHeader(_dim);

        List<float> allX = new List<float>();
        List<float> allY = new List<float>();
        List<float> allZ = new List<float>();

        for (int i = 0; i < strokes.Count; i++) {
            for (int j = 0; j<strokes[i].points.Count; j++) {
                allX.Add(strokes[i].points[j].x);
                allY.Add(strokes[i].points[j].y);
                allZ.Add(strokes[i].points[j].z);
            }
        }
        allX.Sort();
        allY.Sort();
        allZ.Sort();
        maxPoint = new Vector3(allX[allX.Count-1], allY[allY.Count - 1], allZ[allZ.Count - 1]);
        minPoint = new Vector3(allX[0], allY[0], allZ[0]);

        for (int i=0; i<strokes.Count; i++) {
            returns += gmlStroke(strokes[i].points);
        }

        returns += gmlFooter();
        return returns;
    }

    string gmlHeader(Vector3 _dim) {
        string s = "<gml spec=\"0.1b\">" + "\r";
        s += "\t<tag>" + "\r";
        s += "\t\t<header>" + "\r";
        s += "\t\t\t<client>" + "\r";
        s += "\t\t\t\t<name>" + appName + "</name>" + "\r";
        s += "\t\t\t</client>" + "\r";
        s += "\t\t\t<environment>" + "\r";
        s += "\t\t\t\t<up>" + "\r";
        s += "\t\t\t\t\t<x>0</x>" + "\r";
        s += "\t\t\t\t\t<y>1</y>" + "\r";
        s += "\t\t\t\t\t<z>0</z>" + "\r";
        s += "\t\t\t\t</up>" + "\r";
        s += "\t\t\t\t<screenBounds>" + "\r";
        s += "\t\t\t\t\t<x>" + _dim.x + "</x>" + "\r";
        s += "\t\t\t\t\t<y>" + _dim.y + "</y>" + "\r";
        s += "\t\t\t\t\t<z>" + _dim.z + "</z>" + "\r";
        s += "\t\t\t\t</screenBounds>" + "\r";
        s += "\t\t\t</environment>" + "\r";
        s += "\t\t</header>" + "\r";
        s += "\t\t<drawing>" + "\r";
        return s;
     }

    string gmlFooter() {
        string s = "\t\t</drawing>" + "\r";
        s += "\t</tag>" + "\r";
        s += "</gml>" + "\r";
        return s;
    }

    string gmlStroke(List<Vector3> points) {
        string s = "\t\t\t<stroke>" + "\r";
        for (int i=0; i<points.Count; i++) {
            s += gmlPoint(points[i]);
        }
        s += "\t\t\t</stroke>" + "\r";
        return s;
    }

    string gmlPoint(Vector3 point) {
        string s = "\t\t\t\t<pt>" + "\r";
        Vector3 p = normalizePoint(point);
        s += "\t\t\t\t\t<x>" + p.x + "</x>" + "\r";
        s += "\t\t\t\t\t<y>" + p.y + "</y>" + "\r";
        s += "\t\t\t\t\t<z>" + p.z + "</z>" + "\r";
        s += "\t\t\t\t\t<time>" + timeCounter + "</time>" + "\r";
        s += "\t\t\t\t</pt>" + "\r";
        timeCounter += timeIncrement;
        return s;
    }

    Vector3 normalizePoint(Vector3 point) {
        float x = remap(point.x, minPoint.x, maxPoint.x, 0f, 1f);
        float y = remap(point.y, minPoint.y, maxPoint.y, 1f, 0f);
        float z = remap(point.z, minPoint.z, maxPoint.z, 0f, 1f);
        return new Vector3(x, y, z);
    }

    float remap(float value, float from1, float to1, float from2, float to2) {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }

}
