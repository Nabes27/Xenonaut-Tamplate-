using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    [Header("Music Settings")]
    public List<AudioClip> musicTracks;     // Daftar semua musik
    public AudioSource audioSource;         // AudioSource pemutar musik
    public bool playOnStart = true;         // Auto play saat mulai
    public bool loopPlaylist = true;        // Ulang playlist setelah habis

    private List<int> playedIndices = new List<int>(); // Track lagu yang sudah diputar

    void Start()
    {
        if (playOnStart && musicTracks.Count > 0 && audioSource != null)
        {
            PlayRandomTrack();
        }
    }

    void Update()
    {
        if (audioSource != null && !audioSource.isPlaying && musicTracks.Count > 0)
        {
            PlayRandomTrack();
        }
    }

    void PlayRandomTrack()
    {
        if (musicTracks.Count == 0 || audioSource == null) return;

        // Semua lagu sudah diputar
        if (playedIndices.Count >= musicTracks.Count)
        {
            if (loopPlaylist)
            {
                playedIndices.Clear(); // Reset untuk ulang acak
            }
            else
            {
                Debug.Log("Playlist selesai. Tidak ada lagi lagu untuk diputar.");
                return;
            }
        }

        // Pilih lagu yang belum dimainkan
        int index;
        do
        {
            index = Random.Range(0, musicTracks.Count);
        } while (playedIndices.Contains(index));

        playedIndices.Add(index);
        audioSource.clip = musicTracks[index];
        audioSource.Play();

        Debug.Log("Memutar lagu: " + musicTracks[index].name);
    }
}
