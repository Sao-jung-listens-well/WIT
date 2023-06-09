using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using System.Net;
using UnityEditor;

[Serializable]
public class AmendUser
{
	public int member_idx;
}

[Serializable]
public class AmendInfo
{
	public long idx;
	public string amends;
	public int goal;
	public int remain;
}

public class AmendGetInfo
{
	public long idx;
	public string amends;
	public int goal;
	public int remain;
	public AmendGetInfo()
	{
		
	}
	public AmendGetInfo(long idx, string amends, int goal, int remain) 
	{
		this.idx = idx;
		this.amends = amends;
		this.goal = goal;
		this.remain = remain;
	}
	public AmendGetInfo(AmendInfo res)
	{
		res.idx = idx;
		res.amends = amends;
		res.goal = goal;
		res.remain = remain;
	}
}

public class AmendDirector : MonoBehaviour
{
	public static AmendGetInfo aminfo = new AmendGetInfo();
	InputField inputPrize;
	InputField inputPeriod;
	Button btSendInfo;
	// Start is called before the first frame update
	void Start()
    {
		inputPrize = GameObject.Find("inputPrize").GetComponent<InputField>();
		inputPeriod = GameObject.Find("inputPeriod").GetComponent<InputField>();
		btSendInfo = GameObject.Find("ButtonSend").GetComponent<Button>();
		GetAmendInfo();
		if (aminfo.amends == "")
			GameObject.Find("inputPrize").GetComponent<InputField>().placeholder.GetComponent<Text>().text = "상품을 입력해주세요.";
		else
			GameObject.Find("inputPrize").GetComponent<InputField>().placeholder.GetComponent<Text>().text = aminfo.amends;

		if (aminfo.amends == "")
			GameObject.Find("inputPeriod").GetComponent<InputField>().placeholder.GetComponent<Text>().text = "상품수여주기를 입력해주세요.";
		else
			GameObject.Find("inputPeriod").GetComponent<InputField>().placeholder.GetComponent<Text>().text = aminfo.remain.ToString();
		
		btSendInfo.onClick.AddListener(CheckValidation);
	}
	
	void CheckValidation()
	{
		if (inputPrize.text == "" || aminfo.amends == ""){
			//Toast.MakeToast("상품 이름을 입력해주세요.");
			Debug.Log("상품 이름을 입력해주세요.");
			return;
		}
		if (inputPeriod.text == ""){
			//Toast.MakeToast("상품 수여 주기를 입력해주세요.");
			Debug.Log("상품 수여 주기를 입력해주세요.");
			return;
		}
		SendAmendInfo();
	}

	void GetAmendInfo()
	{
		AmendUser info = new AmendUser();
		info.member_idx = UserInfo.GetUserIdx();

		string apiUrl = "http://ec2-43-201-246-145.ap-northeast-2.compute.amazonaws.com/getAmends?member_idx="+ info.member_idx;

		string str = JsonUtility.ToJson(info);
		byte[] bytes = System.Text.Encoding.UTF8.GetBytes(str);

		HttpWebRequest request = (HttpWebRequest)WebRequest.Create(apiUrl);
		request.Method = "GET";

		try
		{
			HttpWebResponse response = (HttpWebResponse)request.GetResponse();
			StreamReader reader = new StreamReader(response.GetResponseStream());
			//Debug.Log("read");
			string json = reader.ReadToEnd();

			AmendInfo result = JsonUtility.FromJson<AmendInfo>(json);
			aminfo.amends = result.amends;
			aminfo.idx = result.idx;
			aminfo.goal = result.goal;
			aminfo.remain = result.remain;
		}
		catch (WebException e)
		{
		}
	}

	void SendAmendInfo()
	{
		string apiUrl = "http://ec2-43-201-246-145.ap-northeast-2.compute.amazonaws.com/amends/update";

		AmendInfo info = new AmendInfo();
		info.idx = aminfo.idx;
		info.amends = inputPrize.text;
		info.goal = Convert.ToInt32(inputPeriod.text);
		info.remain = info.goal;

		string str = JsonUtility.ToJson(info);
		byte[] bytes = System.Text.Encoding.UTF8.GetBytes(str);

		HttpWebRequest request = (HttpWebRequest)WebRequest.Create(apiUrl);
		request.Method = "POST";
		request.ContentType = "application/json";
		request.ContentLength = bytes.Length;

		try
		{
			using (var stream = request.GetRequestStream())
			{
				stream.Write(bytes, 0, bytes.Length);
				stream.Flush();
				stream.Close();
			}
			HttpWebResponse response = (HttpWebResponse)request.GetResponse();
			StreamReader reader = new StreamReader(response.GetResponseStream());

			string json = reader.ReadToEnd();

			if (response.StatusCode == HttpStatusCode.OK)
			{
				AmendInfo data = JsonUtility.FromJson<AmendInfo>(json);

				UserInfo.SetAmends(new Amends((int)data.idx, data.amends, data.goal, data.remain));

				//Toast.MakeToast("보상이 저장되었습니다. ["+data.amends+"/"+data.goal+"]");
			}
			else
			{
				//Toast.MakeToast(text);
			}
		}
		catch (WebException e)
		{
			//Toast.MakeToast("에러 발생");
		}
	}
}
