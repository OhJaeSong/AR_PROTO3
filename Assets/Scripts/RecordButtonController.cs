using Boomlagoon.JSON;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;

public class RecordButtonController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    // 변수 정보를 갖고 있는 Bean 클래스. 자바 Bean 개념을 갖고 오긴 했는데, 추후 Unity 철학에 맞춰 수정 필요.
    private VariablesController variablesController;
    private UtilController utilController;

    private AudioSource outAudioSource;

    private AudioClip recordingAudioClip;
    private float startRecordingTime;

    void Start()
    {
        variablesController = GameObject.Find("recordButton").GetComponent<VariablesController>();
        utilController = GameObject.Find("recordButton").GetComponent<UtilController>();
        outAudioSource = GameObject.Find("outAudioSource").GetComponent<AudioSource>();

        try
        {
            // 개발 속성이 true인 경우, 화면에 오류창이 표시됨.
            utilController.initDevLog();

            //  STT 관련 경로 정보 없을 경우, 경로를 생성한다.
            if (Directory.Exists(variablesController.getStrSttFilePath()) == false)
            {
                Directory.CreateDirectory(variablesController.getStrSttFilePath());
            }

            //  TTS 관련 경로 정보 없을 경우, 경로를 생성한다.
            if (Directory.Exists(variablesController.getStrTtsFilePath()) == false)
            {
                Directory.CreateDirectory(variablesController.getStrTtsFilePath());
            }
        } catch (Exception e)
        {
            utilController.setLog(e.ToString());
            utilController.playErrorAudio("Init_Error", false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        // 프레임마다 현재 녹음 진행중인지 체크해서, 시간 오버될 경우, 강제로 녹음을 종료하고 에러 메시지를 출력한다.
        if (variablesController.isBooRecordYn())
        {
            // 계속 누르고 있는 시간이 최대 음성 입력 시간을 초과할 경우 녹음을 멈춘다.
            if ((Time.time - startRecordingTime) >= variablesController.getFloSttMaxSecond())
            {
                // 이미지 전환 처리. record 이미지를 가져와서 버튼에 할당한다.
                this.GetComponent<Image>().sprite = Resources.Load<Sprite>("record") as Sprite;

                // 마이크 종료 처리.
                Microphone.End("");

                // 녹음 여부 변수 false 처리
                variablesController.setBooRecordYn(false);

                utilController.playErrorAudio("Long_Error", false);
            }
        }
    }

    // 버튼을 누르는 순간 이벤트 처리. 녹음을 시작한다.
    public void OnPointerDown(PointerEventData eventData)
    {
        utilController.initRecord();

        // 이미지 전환 처리. recording 이미지를 가져와서 버튼에 할당한다.
        this.GetComponent<Image>().sprite = Resources.Load<Sprite>("recording") as Sprite;

        // 녹음 시작. 용량을 줄이려면 Sample Rate를 줄이면 되긴 하는데, 음성이 나빠져서 인식율이 떨어진다. 테스트 통해서 좀 더 찾아보는게 좋을 듯
        // 파일 용량은 아래 참조. https://www.colincrawley.com/audio-file-size-calculator/ 
        try
        {
            int minFreq;
            int maxFreq;
            Microphone.GetDeviceCaps("", out minFreq, out maxFreq);
            if (maxFreq < variablesController.getIntFrequence() && maxFreq > 0)
                variablesController.setIntFrequence(maxFreq);

            recordingAudioClip = Microphone.Start("", false, (int)variablesController.getFloSttMaxSecond() + 1, variablesController.getIntFrequence()); // 녹음 시간은 최대초 + 1초 해서 제한함.
            startRecordingTime = Time.time;
        } catch (Exception e)
        {
            // 마이크에 권한이 없거나 문제가 있을 경우 여기서 에러 발생함.
            utilController.setLog(e.ToString());
            utilController.playErrorAudio("Mic_Error", false);
        }
        
    }

    // 버튼을 띄는 순간 이벤트 처리. 녹음을 끝내고, 음성 채팅을 시작한다.
    public void OnPointerUp(PointerEventData eventData)
    {
        variablesController.setBooPressYn(false);

        if (variablesController.isBooRecordYn())
        {
            variablesController.setBooRecordYn(false);

            // 이미지 전환 처리. record 이미지를 가져와서 버튼에 할당한다.
            this.GetComponent<Image>().sprite = Resources.Load<Sprite>("record") as Sprite;

            // 마이크 종료 처리.
            Microphone.End("");

            if (Time.time - startRecordingTime > variablesController.getFloSttMinSecond()){
                //Trim the audioclip by the length of the recording
                AudioClip recordingAudioClipNew = AudioClip.Create(recordingAudioClip.name, (int)((Time.time - startRecordingTime) * recordingAudioClip.frequency), recordingAudioClip.channels, recordingAudioClip.frequency, false);
                float[] data = new float[(int)((Time.time - startRecordingTime) * recordingAudioClip.frequency)];
                recordingAudioClip.GetData(data, 0);
                recordingAudioClipNew.SetData(data, 0);
                this.recordingAudioClip = recordingAudioClipNew;

                GameObject tmpAudioSource = Instantiate(Resources.Load("Prefab/inAudioSourcePrefab"), new Vector3(0, 0, 0), Quaternion.identity) as GameObject;

                tmpAudioSource.GetComponent<AudioSource>().clip = recordingAudioClip;

                string sttFileName = utilController.getCurrentTimeFormat("yyyyMMddHHmmss");

                //음성 길이가 너무 작을 경우, 예외로 빼서 사전에 녹음되어 있는 대화가 나오게끔 처리.
                //ex) 잘 못들었어요. 다시 말씀해 주시겠어요
                if (recordingAudioClip.length < variablesController.getFloSttMinSecond())
                {
                    utilController.playErrorAudio("Short_Error", false);
                    //prefab을 통해 생성한 임시 입력 오디오 소스를 삭제 처리
                    Destroy(tmpAudioSource);
                }
                else if (recordingAudioClip.length > variablesController.getFloSttMaxSecond())
                {
                    utilController.playErrorAudio("Long_Error", false);
                    //prefab을 통해 생성한 임시 입력 오디오 소스를 삭제 처리
                    Destroy(tmpAudioSource);
                }
                else
                {
                    // 파일로 생성
                    SavWav.Save("STT/" + sttFileName, tmpAudioSource.GetComponent<AudioSource>().clip);

                    //naverSttASyncSend("Happy.mp3");
                    naverSttASyncSend(sttFileName + ".wav");

                    //prefab을 통해 생성한 임시 입력 오디오 소스를 삭제 처리
                    Destroy(tmpAudioSource);
                }
            } else
            {
                utilController.playErrorAudio("Short_Error", false);
            }
        }
    }

    /* NAVER STT 관련 시작 */
    // 비동기식
    public void naverSttASyncSend(string fileName)
    {
        try
        {
            string url = variablesController.getStrNaverSttUrl() + variablesController.getStrSttLang();
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Headers.Add("X-NCP-APIGW-API-KEY-ID", variablesController.getStrNaverClientId());
            request.Headers.Add("X-NCP-APIGW-API-KEY", variablesController.getStrNaverSecretKey());
            request.Method = "POST";
            request.ContentType = "application/octet-stream";
            FileStream fs = new FileStream(Path.Combine(variablesController.getStrSttFilePath(), fileName), FileMode.Open, FileAccess.Read);
            byte[] fileData = new byte[fs.Length];
            fs.Read(fileData, 0, fileData.Length);
            fs.Close();

            request.ContentLength = fileData.Length;

            using (Stream requestStream = request.GetRequestStream())
            {
                requestStream.Write(fileData, 0, fileData.Length);
                requestStream.Close();
            }

            request.BeginGetResponse(naverSttASyncSendCallBack, request);
        }
        catch (Exception e)
        {
            utilController.setLog(e.ToString());
            utilController.playErrorAudio("STT_Error", false);
        }
    }

    // 비동기 콜백
    private void naverSttASyncSendCallBack(IAsyncResult ar)
    {
        HttpWebRequest request = (HttpWebRequest)ar.AsyncState;
        HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(ar);
        Stream stream = response.GetResponseStream();
        string status = response.StatusCode.ToString();
        if (status.Equals("OK"))
        {
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            string retJson = reader.ReadToEnd();
            utilController.setLog("devLog1", retJson.ToString());

            stream.Close();
            response.Close();
            reader.Close();

            JSONObject parsedObject = JSONObject.Parse(retJson);

            if (parsedObject.GetString("text") != "")
            {
                naverChatBotASyncSend(parsedObject.GetString("text"));
            }
            else
            {
                utilController.playErrorAudio("STT_Error", false);
            }
        }
        else
        {
            utilController.playErrorAudio("STT_Error", false);
        }
    }

    /* NAVER STT 관련 종료 */

    /* 네이버 Chatbot 관련 시작 */
    // 비동기식
    public void naverChatBotASyncSend(string sendMsg)
    {
        try
        {
            String apiURL = variablesController.getStrNaverChatBotUrl();
            String secretKey = variablesController.getStrNaverChatBotSecretKey();

            string message = getNaverChatBotJsonData(sendMsg);
            String encodeBase64String = makeSignature(message, secretKey);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(apiURL);
            request.Method = "POST";
            request.Headers.Add("X-NCP-CHATBOT_SIGNATURE", encodeBase64String); // Client Secret
            request.ContentType = "application/json;UTF-8";

            using (Stream requestStream = request.GetRequestStream())
            {
                requestStream.Write(Encoding.UTF8.GetBytes(message), 0, Encoding.UTF8.GetBytes(message).Length);
                requestStream.Close();
            }
            request.BeginGetResponse(naverChatBotASyncSendCallBack, request);
        }
        catch (Exception e)
        {
            utilController.setLog(e.ToString());
            utilController.playErrorAudio("ChatBot_Error", false);
        }
    }

    // 비동기 콜백
    private void naverChatBotASyncSendCallBack(IAsyncResult ar)
    {
        HttpWebRequest request = (HttpWebRequest)ar.AsyncState;
        HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(ar);
        Stream stream = response.GetResponseStream();
        string status = response.StatusCode.ToString();
        if (status.Equals("OK"))
        {
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            string retJson = reader.ReadToEnd();
            utilController.setLog("devLog2", retJson.ToString());
            stream.Close();
            response.Close();
            reader.Close();

            string intent = getNaverChatBotParsingIntent(retJson);

            if ("".Equals(intent) || "Normal".Equals(intent))
            {
                intent = "Talking";
            }

            variablesController.setStrIntendTrigger(intent.Replace("\"", "") + "Trigger");

            naverTtsASyncSend(getNaverChatBotParsingText(retJson));
        }
        else
        {
            utilController.playErrorAudio("ChatBot_Error", false);
        }
    }

    public string makeSignature(string message, string secretKey)
    {
        string encodeBase64String = "";

        string secretAccessKey = secretKey;
        string data = message;
        byte[] encSecretKey = Encoding.UTF8.GetBytes(secretAccessKey);
        HMACSHA256 hmac = new HMACSHA256(encSecretKey);
        hmac.Initialize();
        byte[] bytes = Encoding.UTF8.GetBytes(data);
        byte[] rawHmac = hmac.ComputeHash(bytes);
        encodeBase64String = Convert.ToBase64String(rawHmac);

        return encodeBase64String;
    }

    public string getNaverChatBotJsonData(string sendMsg)
    {
        string retVal = "";

        JSONObject data_obj = new JSONObject();
        data_obj.Add("description", sendMsg); //필수 : 사용자로부터 받은 실제 대화 값

        JSONObject bubbles_obj = new JSONObject();
        bubbles_obj.Add("data", data_obj);
        bubbles_obj.Add("type", "text");

        JSONArray bubbles_array = new JSONArray();
        bubbles_array.Add(bubbles_obj);

        JSONObject rstJsonObject = new JSONObject();
        rstJsonObject.Add("version", "v2"); // 필수 : 임의의 버전으로 일단 테스트
        rstJsonObject.Add("userId", "1"); // 필수 : 사용자ID와 같은 사용자별 고유한 값을 보내주게 되어 있음. 다양한 사용자의 접촉 이력 등 로그 관리 등에 이용하면 되나, 현재는 무의미
        //rstJsonObject.Add("userIp", "8.8.8.8"); // 선택
        rstJsonObject.Add("timestamp", 1234); // 필수 : 특정 포멧으로 포멧팅 해줘야 하는데, 굳이 꼭 안해줘도 테스트 가능하니깐 일단 그냥 임의의 값으로 했음.
        rstJsonObject.Add("bubbles", bubbles_array);
        rstJsonObject.Add("event", "send");

        retVal = rstJsonObject.ToString();
        return retVal;
    }


    // 챗봇에서 받은 메시지를 파싱해서, 실제 얻고자 하는 대화값만 가져오는 메서드
    // 추후 시나리오나, 다른 값들을 사용하고자 할 경우, 아래 내용을 수정하면 된다.
    public string getNaverChatBotParsingText(string retJson)
    {
        JSONObject parsedObject = JSONObject.Parse(retJson);
        JSONArray bubbleArray = parsedObject.GetArray("bubbles");
        JSONObject parsedObject2 = JSONObject.Parse(bubbleArray[0].ToString());
        JSONObject parsedObject3 = parsedObject2.GetObject("data");

        return parsedObject3.GetString("description");
    }

    public string getNaverChatBotParsingIntent(string retJson)
    {
        JSONObject parsedObject = JSONObject.Parse(retJson);
        JSONObject parsedObject1 = parsedObject.GetObject("scenario");
        JSONArray parsedObject2 = parsedObject1.GetArray("intent");

        if (parsedObject2.Length > 0)
        {
            return parsedObject2[0].ToString().Replace("\"", "");
        }
        else
        {
            return "";
        }
    }

    /* 네이버 Chatbot 관련 종료 */

    /* 네이버 TTS 관련 시작 */
    // 비동기식
    public void naverTtsASyncSend(string text)
    {
        if ("잘 이해하지 못했어요. 쉽게 설명해주시면 더 좋은 대답을 할 수 있어요.".Equals(text))
        {
            utilController.playErrorAudio("Chatbot_NoMessage_Error", false);
            return;
        }


        try
        {
            string url = variablesController.getStrNaverTtsUrl();
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Headers.Add("X-NCP-APIGW-API-KEY-ID", variablesController.getStrNaverClientId());
            request.Headers.Add("X-NCP-APIGW-API-KEY", variablesController.getStrNaverSecretKey());
            request.Method = "POST";

            string param = "speaker=" + variablesController.getStrTtsSpeaker()
                        + "&volume=" + variablesController.getStrTtsVolume()
                        + "&speed=" + variablesController.getStrTtsSpeed()
                        + "&pitch=" + variablesController.getStrTtsPitch()
                        + "&emotion=" + variablesController.getStrTtsEmotion()
                        + "&format=" + variablesController.getStrTtsFormat()
                        + "&text=" + text;

            byte[] byteDataParams = Encoding.UTF8.GetBytes(param);
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = byteDataParams.Length;

            using (Stream requestStream = request.GetRequestStream())
            {
                requestStream.Write(byteDataParams, 0, byteDataParams.Length);
                requestStream.Close();
            }
            request.BeginGetResponse(naverTtsASyncSendCallBack, request);
        }
        catch (Exception e)
        {
            utilController.setLog(e.ToString());
            utilController.playErrorAudio("TTS_Error", false);
        }
    }

    // 비동기 콜백
    public void naverTtsASyncSendCallBack(IAsyncResult ar)
    {
        HttpWebRequest request = (HttpWebRequest)ar.AsyncState;
        HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(ar);
        string status = response.StatusCode.ToString();
        if (status.Equals("OK"))
        {
            string ttsFile = Path.Combine(variablesController.getStrTtsFilePath(), utilController.getCurrentTimeFormat("yyyyMMddHHmmss") + ".mp3");

            using (Stream output = File.OpenWrite(ttsFile))
            using (Stream input = response.GetResponseStream())
            {
                input.CopyTo(output);
            }

            utilController.setLog("devLog3", ttsFile);

            //단순하게 생각한다면, 원래 아래 부분에다가 음성 재생 부분 넣으면 되는데, 그게 잘 안 되서 UtilController의 update 부분에 그 부분을 넘겨버림.
            //Unity에서는 특정 클래스나 메서드는 Async 방식에서는 사용이 안 되는 듯함.
            //이에 따라, 파일 재생은 실시간 재생이 아닌, Queue 방식으로 구현함.
            //추후 음성 단건 재생이 아닌, 다건 list 형태의 재생이 필요한 경우를 대비한 확장 형태
            //원래대로라면, 기존 Static 형태의 Queue에 넣어야하는데, 일단 getter/setter방식으로 구현. 추후 필요시 serialize관련된 부분도 고려해볼 필요 있음.
            Queue<string> tmpQueue = new Queue<string>();
            tmpQueue = variablesController.getQuePlayFiles();
            tmpQueue.Enqueue(ttsFile);
            variablesController.setQuePlayFiles(tmpQueue);
        }
        else
        {
            utilController.playErrorAudio("TTS_Error", false);
        }
    }

    /* 네이버 TTS 관련 종료 */
}
