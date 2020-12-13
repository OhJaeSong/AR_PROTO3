using System.Collections.Generic;
using UnityEngine;

public class VariablesController : MonoBehaviour
{
	private VariablesController variablesController;
	private UtilController utilController;

	public bool allLoadYn;                      //해당 스크립트 전체 실행 여부

	public bool boolDevYn;						//개발 상태 여부. 개발 파라미터 및 Debugging 상태 관리를 위해
	
	private bool booPressYn;					//버튼 Press 여부
	private bool booRecordYn;                   //녹음 진행 여부
	private bool booInAudioPlayingYn;			//입력 오디오소스 사용 여부
	private bool booOutAudioPlayingYn;			//출력 오디오소스 사용 여부

	private float floSttMinSecond;				//녹음 시 음성 인식 최소 길이(초)
	private float floSttMaxSecond;				//녹음 시 음성 인식 최대 길이(초)

	private int intFrequence;					//녹음 시 음성파일 비트레이트

	private string strSttFilePath;              //STT 파일 생성 경로
	private string strTtsFilePath;              //TTS 파일 생성 경로
	private string strNaverSttUrl;				//네이버 STT URL
	private string strNaverTtsUrl;				//네이버 TTS URL
	private string strNaverClientId;			//네이버 STT,TTS ClientID
	private string strNaverSecretKey;           //네이버 STT,TTS SecretKey
	private string strNaverChatBotUrl;          //네이버 챗봇 URL
	private string strNaverChatBotSecretKey;	//네이버 챗봇 SecretKey
	private string strSttLang;					//네이버 STT 언어 설정
	private string strTtsSpeaker;				//네이버 TTS 화자 설정
	private string strTtsVolume;				//네이버 TTS 볼륨 설정
	private string strTtsSpeed;					//네이버 TTS 속도 설정
	private string strTtsPitch;					//네이버 TTS 어조 설정
	private string strTtsEmotion;				//네이버 TTS 감정 설정
	private string strTtsFormat;                //네이버 TTS 파일 포멧 설정

	private string strIntendTrigger;            

	private string strLogText1;
	private string strLogText2;
	private string strLogText3;
	private string strErrText;

	private Queue<string> quePlayFiles;

	void Start()
	{
		boolDevYn				= true;

		quePlayFiles			= new Queue<string>();

		booPressYn				= false;
		booRecordYn				= false;
		booInAudioPlayingYn		= false;
		booOutAudioPlayingYn	= false;

		floSttMinSecond			= 0.5f;
		floSttMaxSecond			= 5.0f;

		intFrequence			= 14400;

		strSttFilePath			= Application.persistentDataPath + "/STT" ; // Android의 경우 내부 데이터 영역에 파일 생성됨. 아이폰 계열의 경우 파일 생성 위치 확인 필요
		strTtsFilePath			= Application.persistentDataPath + "/TTS"; // Android의 경우 내부 데이터 영역에 파일 생성됨. 아이폰 계열의 경우 파일 생성 위치 확인 필요
		strNaverSttUrl			= "https://naveropenapi.apigw.ntruss.com/recog/v1/stt?lang=";
		strNaverTtsUrl			= "https://naveropenapi.apigw.ntruss.com/voice-premium/v1/tts";
		strNaverClientId		= "9vvfkq7nls";
		strNaverSecretKey		= "ciWc0V5As7ZJS6JpxYJ46KAo2YC2cy0J8FNFAKdP";
		strNaverChatBotUrl		= "https://gy8aopv5rm.apigw.ntruss.com/send/betaStage/";
		strNaverChatBotSecretKey = "cHZnRWJZaXBRTlNQbmJqRnNldXJNamdnQ1N2TEVxcGo=";
		strSttLang				= "Kor";
		strTtsSpeaker			= "nara";
		strTtsVolume			= "0";
		strTtsSpeed				= "0";
		strTtsPitch				= "0";
		strTtsEmotion			= "0";
		strTtsFormat			= "mp3";

		strLogText1				= "";
		strLogText2				= "";
		strLogText3				= "";
		strErrText				= "";

		strIntendTrigger		= "-1";

		allLoadYn				= true;
}

	// Update is called once per frame
	void Update()
	{
		// 매 프레임 단위로 입출력 오디오 소스의 사용 여부를 갱신
		booInAudioPlayingYn		= GameObject.Find("inAudioSource").GetComponent<AudioSource>().isPlaying;
		booOutAudioPlayingYn	= GameObject.Find("outAudioSource").GetComponent<AudioSource>().isPlaying;
	}

	public bool isBoolDevYn()
	{
		return boolDevYn;
	}

	public void setBoolDevYn(bool boolDevYn)
	{
		this.boolDevYn = boolDevYn;
	}
	

	public bool isBooPressYn()
	{
		return booPressYn;
	}

	public void setBooPressYn(bool booPressYn)
	{
		this.booPressYn = booPressYn;
	}

	public bool isBooRecordYn()
	{
		return booRecordYn;
	}

	public void setBooRecordYn(bool booRecordYn)
	{
		this.booRecordYn = booRecordYn;
	}

	public bool isBooInAudioPlayingYn()
	{
		return booInAudioPlayingYn;
	}

	public void setBooInAudioPlayingYn(bool booInAudioPlayingYn)
	{
		this.booInAudioPlayingYn = booInAudioPlayingYn;
	}

	public bool isBooOutAudioPlayingYn()
	{
		return booOutAudioPlayingYn;
	}

	public void setBooOutAudioPlayingYn(bool booOutAudioPlayingYn)
	{
		this.booOutAudioPlayingYn = booOutAudioPlayingYn;
	}

	public float getFloSttMinSecond()
	{
		return floSttMinSecond;
	}

	public void setFloSttMinSecond(float floSttMinSecond)
	{
		this.floSttMinSecond = floSttMinSecond;
	}

	public float getFloSttMaxSecond()
	{
		return floSttMaxSecond;
	}

	public void setFloSttMaxSecond(float floSttMaxSecond)
	{
		this.floSttMaxSecond = floSttMaxSecond;
	}

	public int getIntFrequence()
	{
		return intFrequence;
	}

	public void setIntFrequence(int intFrequence)
	{
		this.intFrequence = intFrequence;
	}

	public string getStrSttFilePath()
	{
		return strSttFilePath;
	}

	public void setStrSttFilePath(string strSttFilePath)
	{
		this.strSttFilePath = strSttFilePath;
	}

	public string getStrTtsFilePath()
	{
		return strTtsFilePath;
	}

	public void setStrTtsFilePath(string strTtsFilePath)
	{
		this.strTtsFilePath = strTtsFilePath;
	}

	public string getStrNaverSttUrl()
	{
		return strNaverSttUrl;
	}

	public void setStrNaverSttUrl(string strNaverSttUrl)
	{
		this.strNaverSttUrl = strNaverSttUrl;
	}

	public string getStrNaverTtsUrl()
	{
		return strNaverTtsUrl;
	}

	public void setStrNaverTtsUrl(string strNaverTtsUrl)
	{
		this.strNaverTtsUrl = strNaverTtsUrl;
	}

	public string getStrNaverClientId()
	{
		return strNaverClientId;
	}

	public void setStrNaverClientId(string strNaverClientId)
	{
		this.strNaverClientId = strNaverClientId;
	}

	public string getStrNaverSecretKey()
	{
		return strNaverSecretKey;
	}

	public void setStrNaverSecretKey(string strNaverSecretKey)
	{
		this.strNaverSecretKey = strNaverSecretKey;
	}

	public string getStrNaverChatBotUrl()
	{
		return strNaverChatBotUrl;
	}

	public void setStrNaverChatBotUrl(string strNaverChatBotUrl)
	{
		this.strNaverChatBotUrl = strNaverChatBotUrl;
	}

	public string getStrNaverChatBotSecretKey()
	{
		return strNaverChatBotSecretKey;
	}

	public void setStrNaverChatBotSecretKey(string strNaverChatBotSecretKey)
	{
		this.strNaverChatBotSecretKey = strNaverChatBotSecretKey;
	}

	public string getStrSttLang()
	{
		return strSttLang;
	}

	public void setStrSttLang(string strSttLang)
	{
		this.strSttLang = strSttLang;
	}

	public string getStrTtsSpeaker()
	{
		return strTtsSpeaker;
	}

	public void setStrTtsSpeaker(string strTtsSpeaker)
	{
		this.strTtsSpeaker = strTtsSpeaker;
	}

	public string getStrTtsVolume()
	{
		return strTtsVolume;
	}

	public void setStrTtsVolume(string strTtsVolume)
	{
		this.strTtsVolume = strTtsVolume;
	}

	public string getStrTtsSpeed()
	{
		return strTtsSpeed;
	}

	public void setStrTtsSpeed(string strTtsSpeed)
	{
		this.strTtsSpeed = strTtsSpeed;
	}

	public string getStrTtsPitch()
	{
		return strTtsPitch;
	}

	public void setStrTtsPitch(string strTtsPitch)
	{
		this.strTtsPitch = strTtsPitch;
	}

	public string getStrTtsEmotion()
	{
		return strTtsEmotion;
	}

	public void setStrTtsEmotion(string strTtsEmotion)
	{
		this.strTtsEmotion = strTtsEmotion;
	}

	public string getStrTtsFormat()
	{
		return strTtsFormat;
	}

	public void setStrTtsFormat(string strTtsFormat)
	{
		this.strTtsFormat = strTtsFormat;
	}


	public string getStrIntendTrigger()
	{
		return strIntendTrigger;
	}

	public void setStrIntendTrigger(string strIntendTrigger)
	{
		this.strIntendTrigger = strIntendTrigger;
	}

	public string getStrLogText1()
	{
		return strLogText1;
	}

	public void setStrLogText1(string strLogText1)
	{
		this.strLogText1 = strLogText1;
	}

	public string getStrLogText2()
	{
		return strLogText2;
	}

	public void setStrLogText2(string strLogText2)
	{
		this.strLogText2 = strLogText2;
	}

	public string getStrLogText3()
	{
		return strLogText3;
	}

	public void setStrLogText3(string strLogText3)
	{
		this.strLogText3 = strLogText3;
	}

	public string getStrErrText()
	{
		return strErrText;
	}

	public void setStrErrText(string strErrText)
	{
		this.strErrText = strErrText;
	}


	public Queue<string> getQuePlayFiles()
	{
		return quePlayFiles;
	}

	public void setQuePlayFiles(Queue<string> quePlayFiles)
	{
		this.quePlayFiles = quePlayFiles;
	}
}
