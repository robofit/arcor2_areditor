using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoPlayerImage : MonoBehaviour {

    public RawImage RawImage;
    public VideoPlayer VideoPlayer;
    
    private Texture originalImageTexture;

    private void Start() {
        originalImageTexture = RawImage.texture;
        VideoPlayer.prepareCompleted += OnVideoPrepared;
    }

    // Puts texture on Image when the video is ready to play
    private void OnVideoPrepared(VideoPlayer source) {
        RawImage.texture = source.texture;
    }

    public void PlayVideo() {
        // Play() will call function Prepare() which triggers event prepareCompleted
        VideoPlayer.Play();
    }

    public void PlayVideo(float playingTime) {
        VideoPlayer.Play();
        Invoke("StopVideo", playingTime);
    }

    public void StopVideo() {
        VideoPlayer.Stop();
        RawImage.texture = originalImageTexture;
    }

}
