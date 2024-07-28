const { ajax, defer, fromEvent, interval, filter, map, catchError, of } = rxjs;


const url = '/api/values';

const getData = (url) => {
    return ajax.ajax.getJSON(url).pipe(
        map(response => response),
        catchError(error => {
            console.error('Error: ', error);
            return of(error);
        })
    );
};

getData(url).subscribe({
    next: data => {
        console.log(data);
    },
    error: err => console.error('Error: ', err),
    complete: () => console.log('Request complete')
});