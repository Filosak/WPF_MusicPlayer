namespace Music_player_working.Model
{
    public class SwapObject
    {
        public SongModel SongA;
        public SongModel SongB;

        public SwapObject(SongModel songa, SongModel songb) 
        {
            SongA = songa;
            SongB = songb;
        }

    }
}
