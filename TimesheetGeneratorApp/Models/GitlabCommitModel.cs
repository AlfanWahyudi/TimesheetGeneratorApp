namespace TimesheetGeneratorApp.Models
{
    public class GitlabCommitModel
    {
        public string? id { get; set; }  
        public string? short_id { get; set; }  
        public DateTime? created_at { get; set; }  
        public string[]? parent_ids { get; set; }  
        public string? title { get; set; }
        public string? message { get; set; }
        public string? author_name { get; set; } 
        public string? author_email { get; set; } 
        public string? authored_date { get; set; } 
        public string? commiter_name { get; set; } 
        public string? commiter_email { get; set; } 
        public DateTime? committed_date { get; set; }
        public string? web_url { get; set; }
    }
}

//{
//    "id": "942e152cb0b763257ee4548dc5c831139998cd45",
//        "short_id": "942e152c",
//        "created_at": "2022-09-15T15:09:18.000+07:00",
//        "parent_ids": [
//            "d3c0d631023e96fa880bf42089752e381e9ca057"
//        ],
//        "title": "feat: tambah filter wilayah dashboard ppg",
//        "message": "feat: tambah filter wilayah dashboard ppg\n",
//        "author_name": "ihsan_akhdani",
//        "author_email": "hhardjadi00@gmail.com",
//        "authored_date": "2022-09-15T15:09:18.000+07:00",
//        "committer_name": "ihsan_akhdani",
//        "committer_email": "hhardjadi00@gmail.com",
//        "committed_date": "2022-09-15T15:09:18.000+07:00",
//        "web_url": "https://dev.kpk.go.id/gitlab/gol/sig-2020/-/commit/942e152cb0b763257ee4548dc5c831139998cd45",
//        "stats": {
//        "additions": 56,
//            "deletions": 8,
//            "total": 64
//        }
//},
