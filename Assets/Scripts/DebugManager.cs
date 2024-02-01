using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DebugManager : MonoBehaviour
{
    [SerializeField]
    private GameObject m_textChatPrefab;
    [SerializeField]
    private Transform m_parentContent;

 	// string myLog;
 	// Queue myLogQueue = new Queue ();

 	void OnEnable () {
 		Application.logMessageReceived += HandleLog;
 	}

 	void OnDisable () {
 		Application.logMessageReceived -= HandleLog;
 	}

 	void HandleLog (string logString, string stackTrace, LogType type) {
 		// myLog = logString;
 		// string newString = "\n [" + type + "] : " + myLog;
 		// myLogQueue.Enqueue (newString);
 		// if (type == LogType.Exception) {
 		// 	newString = "\n" + stackTrace;
 		// 	myLogQueue.Enqueue (newString);
 		// }
 		// myLog = string.Empty;
 		// foreach (string mylog in myLogQueue) {
 		// 	myLog += mylog;
 		// }
		UpdateDebug("[Debug]"+logString + "\n [" + type + "] " +stackTrace);
 	}

 	// void OnGUI () {
 	// 	GUILayout.Label (myLog);
	// }

    public void UpdateDebug(string _str){
        GameObject clone = Instantiate(m_textChatPrefab, m_parentContent);
        clone.GetComponent<TextMeshProUGUI>().text = $"{_str}";
        
    }
}
