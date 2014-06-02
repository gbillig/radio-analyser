using System;
using System.IO;

class Program {
    static void Main() {
        string filenameIn = "songs.csv";
        string filenameOut = "songs_analysed.csv";
        
        StreamReader inStream = null;   
        StreamWriter outStream = null;

        
        if (!File.Exists(filenameIn)) {
            //if filenameIn doesn't exist, throw argumentexception
            throw new ArgumentException("songs.csv does not exist!");
        } else {
            //creating streamreader for LakeTahoeDEM
            inStream = new StreamReader(filenameIn);
        }
        
        outStream = new StreamWriter(filenameOut);
        
        Database data = new Database(inStream);
        
        data.WriteCSV(outStream);
    }

}

class Info {
    string song;
    string artist;
    public Timestamp[] times; // = new string[100];
    int downloaded;
    int freq;

    public Info(string song, string artist, int downloaded, int freq, Timestamp[] times ) {
        this.song = song;
        this.artist = artist;
        this.freq = freq;
        this.times = times;
        this.downloaded = downloaded;   
    }
    
    public string Song {get { return this.song; } }
    public string Artist {get { return this.artist; } }
    
    public int Downloaded {
        get { return this.downloaded; }
        set { this.downloaded = value; }
    }
    
    public int Frequency {
        get { return this.freq; }
        set { this.freq = value; }
    }

    
    public Timestamp this[int num] {   
        get { return times[num]; }
        set { times[num] = value; }
    }
    
}

class Timestamp {
    private int day;
    private int month;
    private int year;
    private int hour;
    private int minute;
    
    public Timestamp(string line) {
        if (line.Length != 14) {
            Console.WriteLine(line);
            throw new ArgumentException("Timestamp must be 14 char long!");
        }
    
        this.day    = (int)Char.GetNumericValue(line[0]) * 10 + (int)Char.GetNumericValue(line[1]);
        this.month  = (int)Char.GetNumericValue(line[3]) * 10 + (int)Char.GetNumericValue(line[4]);
        this.year   = (int)Char.GetNumericValue(line[6]) * 10 + (int)Char.GetNumericValue(line[7]);
        this.hour   = (int)Char.GetNumericValue(line[9]) * 10 + (int)Char.GetNumericValue(line[10]);
        this.minute = (int)Char.GetNumericValue(line[12]) * 10 + (int)Char.GetNumericValue(line[13]);

    }
    
    
    public int Day {get { return this.day; } }
    public int Month {get { return this.month; } }
    public int Year {get { return this.year; } }
    public int Hour {get { return this.hour; } }
    public int Minute {get { return this.minute; } }
    
    
    public override string ToString() {
        return String.Format("{0:00}/{1:00}/{2:00} {3:00}:{4:00}", day, month, year, hour, minute);
    }
    
}

class Database {
    Info[] songs = new Info[1000];
    int length = 0;
    
    public Database(StreamReader inStream) {
        this.length = Count(inStream);
        
        //resets the Stream to beginning of file
        inStream.BaseStream.Seek(0, SeekOrigin.Begin); 
        
        ReadCSV(inStream);
    }
    
    public void ReadCSV(StreamReader inStream) {
        int a = 0;
        int freq;

        string line = inStream.ReadLine();        
        
        for(a=0;a<length;a++) {
             string[] info = line.Split(',');
             freq = Int32.Parse(info[3]);
             
             //original timestamp creation
             //Timestamp[] times = new Timestamp[freq];
             
             Timestamp[] times = new Timestamp[250];
             
             for(int b=0;b<freq;b++)
             {
                times[b] = new Timestamp(info[4+b]);
                //Console.WriteLine("Time #{0}: {1}", b+1,times[b]);
             }
             
             
             songs[a] = new Info(info[0],info[1],Int32.Parse(info[2]),Int32.Parse(info[3]), times);
             
             Console.WriteLine("{0} by {1}", songs[a].Song, songs[a].Artist);
             
             line = inStream.ReadLine();
        }
        
        RemoveDup(songs);
        CombineSongs(songs);
    }
    
    public void WriteCSV(StreamWriter outStream) {
        for (int a = 0; a < length; a++) {
            Console.Write("{0},{1},{2},{3},", songs[a].Song, songs[a].Artist, songs[a].Downloaded, songs[a].Frequency);
            outStream.Write("{0},{1},{2},{3},", songs[a].Song, songs[a].Artist, songs[a].Downloaded, songs[a].Frequency);
           
           for (int b=0; b<songs[a].Frequency; b++) {
                Console.Write("{0},", songs[a][b]);   //song[a].Timestamp[b]);
                outStream.Write("{0},", songs[a][b]); 
            }
            Console.WriteLine();
            outStream.WriteLine();
            
            //outStream.WriteLine("{0},{1},{2},{3},", songs[a].Song, songs[a].Artist, songs[a].Timestamp, songs[a].Downloaded);
        }        
        
        outStream.Close();
    }
    
    private void CombineSongs(Info[] songs) {
    
        for(int a = 0; a < length - 2; a++) {
            for(int b = a + 2; b < length; b++) {
                if (songs[a].Song == songs[b].Song && songs[a].Artist ==  songs[b].Artist) {
                
                    //Array.Resize<Timestamp>(ref songs[a], songs[b].Frequency);
                    
                    for(int c = 0; c < songs[b].Frequency; c++) {
                        songs[a][songs[a].Frequency + c] = new Timestamp(songs[b][c].ToString());
                    }
                
                    songs[a].Frequency += songs[b].Frequency;
                    Delete(songs, b);

                }
                    
            }
        }
    }
    
    
    private void RemoveDup(Info[] songs) {
        
        //Console.WriteLine("Song.Length = {0}", length);
        for(int a = 0; a < length - 1; a++) {
            while (songs[a].Song == songs[a+1].Song && songs[a].Artist ==  songs[a+1].Artist) {
                Delete(songs, a+1);
            }
        }
    }
    
    private void Delete(Info[] songs, int rank) {
        for (int a = rank; a<length - 1; a++)
        {
           songs[a] = songs[a+1];
        }
        length --;
    }
    
    
    private int Count(StreamReader inStream) {
       	string line;
        int a = 0;

	line = inStream.ReadLine();
        
        while(line != null) {
            a++;
            line = inStream.ReadLine();
        }

	return a;
        
    }
}
