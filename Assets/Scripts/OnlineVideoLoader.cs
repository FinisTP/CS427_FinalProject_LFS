using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using YoutubePlayer;

public class OnlineVideoLoader : MonoBehaviour
{

    public VideoPlayer videoPlayer;
    public AudioSource audioSource;
    public string videoUrl = "yourvideourl";

    // Start is called before the first frame update
    void Start()
    {
        // videoPlayer.url = videoUrl;
        // PlayVideo();
        videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
        videoPlayer.EnableAudioTrack(0, true);
        videoPlayer.Prepare();
        videoPlayer.SetTargetAudioSource(0, audioSource);
    }


    public async void PlayVideo()
    {
        // await videoPlayer.PrepareAsync();
        await videoPlayer.PlayYoutubeVideoAsync(videoUrl);
    }
}