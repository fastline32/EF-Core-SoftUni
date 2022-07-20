using System;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using MusicHub.Data;
using System.Linq;
using System.Text;

namespace MusicHub
{
    public class StartUp
    {
        public static void Main()
        {
            var context = new MusicHubDbContext();
            var result = ExportSongsAboveDuration(context, 4);
            Console.WriteLine(result);
        }

        public static string ExportAlbumsInfo(MusicHubDbContext context, int producerId)
        {
            var producer = context.Producers
                .Include(x => x.Albums)
                .ThenInclude(x => x.Songs)
                .ThenInclude(x => x.Writer)
                .FirstOrDefault(x => x.Id == producerId);
            
            var sb = new StringBuilder();
            foreach (var producerAlbum in producer.Albums.OrderByDescending(x => x.Price))
            {
                sb.AppendLine($"-AlbumName: {producerAlbum.Name}");
                sb.AppendLine(
                    $"-ReleaseDate: {producerAlbum.ReleaseDate.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture)}");
                sb.AppendLine($"-ProducerName: {producer.Name}");
                sb.AppendLine("-Songs:");
                int count = 1;
                foreach (var song in producerAlbum.Songs.OrderByDescending(x => x.Name).ThenBy(x => x.Writer.Name))
                {
                    sb.AppendLine($"---#{count}");
                    sb.AppendLine($"---SongName: {song.Name}");
                    sb.AppendLine($"---Price: {song.Price:f2}");
                    sb.AppendLine($"---Writer: {song.Writer.Name}");
                    count++;
                }

                sb.AppendLine($"-AlbumPrice: {producerAlbum.Price:f2}");
            }
            return sb.ToString().TrimEnd();
        }

        public static string ExportSongsAboveDuration(MusicHubDbContext context, int duration)
        {
            var songs = context.Songs.ToList()
                .Where(x => x.Duration.TotalSeconds > duration)
                .Select(x => new
                {
                    SongName = x.Name,
                    Writer = x.Writer.Name,
                    Performer = x.SongPerformers.Select(x => x.Performer.FirstName + " " + x.Performer.LastName).FirstOrDefault(),
                    AlbumProducer = x.Album.Producer.Name,
                    Duration = x.Duration
                });
              

            var sb = new StringBuilder();
            int count = 1;
            foreach (var song in songs.OrderBy(x => x.SongName).ThenBy(x => x.Writer).ThenBy(x => x.Performer))
            {
                sb.AppendLine($"-Song #{count}");
                sb.AppendLine($"---SongName: {song.SongName}");
                sb.AppendLine($"---Writer: {song.Writer}");
                sb.AppendLine($"---Performer: {song.Performer}");
                sb.AppendLine($"---AlbumProducer: {song.AlbumProducer}");
                sb.AppendLine($"---Duration: {song.Duration:c}");
                count++;
            }
            return sb.ToString().TrimEnd();
        }
    }
}
