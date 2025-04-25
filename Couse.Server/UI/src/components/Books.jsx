import { useEffect, useState } from 'react';

function BookClient() {
    const [books, setBooks] = useState([]);
    const [formData, setFormData] = useState({ id: '', title: '', author: '' });

    const soapRequest = (operation, data) => `
        <soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
            <soap:Body>
                <tns:${operation} xmlns:tns="http://tempuri.org/">
                    ${data}
                </tns:${operation}>
            </soap:Body>
        </soap:Envelope>`;

    const parseSoapResponse = async (response) => {
        const text = await response.text();
        const parser = new DOMParser();
        const xml = parser.parseFromString(text, "text/xml");
        const bookNodes = xml.getElementsByTagName("Book");

        const result = [];
        for (let i = 0; i < bookNodes.length; i++) {
            const book = bookNodes[i];
            result.push({
                id: book.getElementsByTagName("id")[0]?.textContent,
                title: book.getElementsByTagName("title")[0]?.textContent,
                author: book.getElementsByTagName("author")[0]?.textContent,
            });
        }

        return result;
    };

    useEffect(() => {
        const fetchBooks = async () => {
            console.log(soapRequest('GetAllBooks', ''));
            const response = await fetch('http://localhost:8080/soap/GetAllBooks', {
                method: 'POST',
                headers: { 'Content-Type': 'text/xml' },
                body: soapRequest('GetAllBooks', '')
            });
            
            const result = await parseSoapResponse(response);
            setBooks(result);
        };
    
        fetchBooks();
    }, []);

    // const createBook = async () => {
    //     const xmlData = `
    //         <tns:title>${formData.title}</tns:title>
    //         <tns:author>${formData.author}</tns:author>
    //     `;

    //     const response = await fetch('http://localhost:8080/soap/CreateBook', {
    //         method: 'POST',
    //         headers: {
    //             'Content-Type': 'text/xml',
    //             'SOAPAction': 'http://tempuri.org/CreateBook'
    //         },
    //         body: soapRequest('CreateBook', xmlData)
    //     });

    //     if (response.ok) {
    //         await fetchBooks(); // Обновляем список
    //         setFormData({ id: '', title: '', author: '' }); // Сбрасываем форму
    //     } else {
    //         console.error("Ошибка при добавлении книги");
    //     }
    // };

    return (
        <div>
            <h1>📚 Книги</h1>

            <input
                type="text"
                placeholder="Название"
                value={formData.title}
                onChange={(e) => setFormData({ ...formData, title: e.target.value })}
            />
            <input
                type="text"
                placeholder="Автор"
                value={formData.author}
                onChange={(e) => setFormData({ ...formData, author: e.target.value })}
            />
            {/* <button onClick={createBook}>➕ Добавить книгу</button> */}

            <ul>
                {books.map((book) => (
                    <li key={book.id}>
                        <strong>{book.title}</strong> — {book.author}
                    </li>
                ))}
            </ul>
        </div>
    );
}

export default BookClient;
