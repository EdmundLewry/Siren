import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Component, OnInit, ViewChild } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { MatTable, MatTableDataSource } from '@angular/material/table';
import { Channel } from '../../interfaces/ichannel';

@Component({
  selector: 'lib-channel-list',
  templateUrl: './channel-list.component.html',
  styleUrls: ['./channel-list.component.css']
})
export class ChannelListComponent implements OnInit {
  public readonly dataSource: MatTableDataSource<Channel> = new MatTableDataSource();
  public readonly displayedColumns: string[] = [
    "id",
    "name",
    "listCount",
    "options"
  ];

  private readonly httpOptions = {
    headers: new HttpHeaders({ 'Content-Type': 'application/json' })
  };
  
  @ViewChild('table') table: MatTable<Channel>
  
  constructor(private http: HttpClient,
    public dialog: MatDialog) {
  }

  ngOnInit(): void {
    this.retrieveChannelInformation();
  }

  private retrieveChannelInformation(): void {
    // this.dataSource.data = [
    //   {
    //     id: 1,
    //     name: "test 1",
    //     listCount: 2,
    //     healthyListCount: 2
    //   },
    //   {
    //     id: 2,
    //     name: "test 2",
    //     listCount: 2,
    //     healthyListCount: 0
    //   },
    // ];
    this.http.get<Channel[]>(`/proxy/api/1/automation/channel`, this.httpOptions).subscribe(result => {
      this.dataSource.data = result;
    }, error => console.error(error));
  }

  public requestAddNewChannel(): void {
    
  }
}
