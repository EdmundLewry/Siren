import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { MatTableDataSource } from '@angular/material/table';

@Component({
  selector: 'lib-transmissionlist-list',
  templateUrl: './transmissionlist-list.component.html',
  styleUrls: ['./transmissionlist-list.component.css']
})
export class TransmissionlistListComponent implements OnInit {

  public readonly dataSource: MatTableDataSource<TransmissionList> = new MatTableDataSource();

  public readonly displayedColumns: string[] = [
    "id",
    "playlistId",
    "eventCount"
  ]

  //private readonly fakeData: TransmissionList[] = [
  //  { id: 1, playlistId: 1, eventCount: 5 },
  //  { id: 2, playlistId: 6, eventCount: 131 },
  //  { id: 3, playlistId: 4, eventCount: 64 },
  //  { id: 4, playlistId: 8, eventCount: 58 },
  //  { id: 5, playlistId: 19, eventCount: 5564 },
  //  { id: 6, playlistId: 2048, eventCount: 21 },
  //];

  constructor(private http: HttpClient) {
    //this.dataSource.data = this.fakeData;
  }

  public ngOnInit() {
    this.http.get<TransmissionList[]>('/proxy/api/1/automation/transmissionlist').subscribe(result => {
      this.dataSource.data = result;
    }, error => console.error(error));
  }
}

interface TransmissionList {
  id: number;
  playlistId: number;
  eventCount: number;
}
