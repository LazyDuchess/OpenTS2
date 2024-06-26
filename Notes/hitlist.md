# HitList resource
This resource is played by the game like any normal sound, but instead of having sound data it contains references to other audio files that will be randomly picked on playback.

They tend to have a PropertySet file alongside them with the same Instance and Group IDs, of Type ID 0x0B9EB87E (Labeled Track Settings by SimPE) which seems to provide data about the sound such as Sample Rate. and references to other Resources.

## Format
```
4 Bytes: Version? Always 56
4 Bytes: Sound count
Loop for Sound Count
  Each entry is a reference to an actual audio file, such as an MP3/XA/WAV/etc. 
  4 Bytes: Instance ID Low
  4 Bytes: Instance ID High
End Loop
```