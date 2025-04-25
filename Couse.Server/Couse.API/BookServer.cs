using System.Security;
using System.Text;

namespace Couse.API;

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Xml;

public class Book
{
    public Guid Id { get; set; }
    
    public string Title { get; set; } = string.Empty;
    
    public string Author { get; set; } = string.Empty;
}

class BookServer
{
    private static readonly List<Book> Books = [];
    
    public static void Main()
    {
        Books.AddRange([
            new Book
            {
                Id = Guid.NewGuid(),
                Title = "Clean Code",
                Author = "Robert C. Martin"
            },
            new Book
            {
                Id = Guid.NewGuid(),
                Title = "Clean Code",
                Author = "Robert C. Martin"
            }
        ]);
        using var listener = new HttpListener();
        listener.Prefixes.Add("http://localhost:8080/soap/");
        listener.Start();
        Console.WriteLine("SOAP Book Server listening on port 8080...");

        while (true)
        {
            var context = listener.GetContext();
            ProcessRequest(context);
        }
    }

    private static void ProcessRequest(HttpListenerContext context)
    {
        var request = context.Request;
        var response = context.Response;

        AddCorsHeaders(response);
        
        if (request.HttpMethod == "OPTIONS")
        {
            response.StatusCode = 200;
            response.Close();
            return;
        }
        
        try
        {
            string soapRequest;
            using (var reader = new StreamReader(request.InputStream))
            {
                soapRequest = reader.ReadToEnd();
            }

            var doc = new XmlDocument();
            doc.LoadXml(soapRequest);

            var ns = new XmlNamespaceManager(doc.NameTable);
            ns.AddNamespace("soap", "http://schemas.xmlsoap.org/soap/envelope/");
            ns.AddNamespace("tns", "http://tempuri.org/");

            var method = request.Url?.Segments[2].Trim('/');
            var httpMethod = request.HttpMethod;

            switch (httpMethod + ":" + method)
            {
                case "POST:CreateBook":
                    HandleCreateBook(doc, ns, response);
                    break;
                // case "POST:GetBook":
                //     HandleGetBook(doc, ns, response);
                //     break;
                // case "POST:UpdateBook":
                //     HandleUpdateBook(doc, ns, response);
                //     break;
                // case "POST:DeleteBook":
                //     HandleDeleteBook(doc, ns, response);
                //     break;
                case "POST:GetAllBooks":
                    HandleGetAllBooks(response);
                    break;
                default:
                    SendSoapFault(response, "Invalid operation");
                    break;
            }
        }
        catch (Exception ex)
        {
            SendSoapFault(response, ex.Message);
        }
    }

    private static void HandleCreateBook(XmlDocument doc, XmlNamespaceManager ns, HttpListenerResponse response)
    {
        var titleNode = doc.SelectSingleNode("//tns:CreateBook/tns:title", ns);
        var authorNode = doc.SelectSingleNode("//tns:CreateBook/tns:author", ns);

        if (titleNode == null || authorNode == null)
        {
            SendSoapFault(response, "Missing required fields");
            return;
        }

        var book = new Book
        {
            Id = Guid.NewGuid(),
            Title = titleNode.InnerText,
            Author = authorNode.InnerText
        };

        Books.Add(book);

        var soapResponse = $"""
                            
                                    <soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
                                        <soap:Body>
                                            <Book xmlns="http://tempuri.org/">
                                                <id>{book.Id}</id>
                                                <title>{book.Title}</title>
                                                <author>{book.Author}</author>
                                            </Book>
                                        </soap:Body>
                                    </soap:Envelope>
                            """;

        SendResponse(response, soapResponse);
    }
    
    private static void SendResponse(HttpListenerResponse response, string soapXml)
    {
        try
        {
            // Преобразуем XML-строку в байтовый массив в UTF-8 кодировке
            var buffer = Encoding.UTF8.GetBytes(soapXml);
        
            // Устанавливаем HTTP-заголовки
            response.ContentType = "text/xml; charset=utf-8"; // MIME-тип для SOAP
            response.ContentLength64 = buffer.Length; // Размер контента в байтах
        
            // Отправляем данные клиенту
            response.OutputStream.Write(buffer, 0, buffer.Length);
        }
        finally
        {
            // Закрываем поток ответа в любом случае (даже при ошибке)
            response.OutputStream.Close();
        }
    }
    
    private static void SendSoapFault(HttpListenerResponse response, string message)
    {
        // Формируем XML с ошибкой в формате SOAP Fault
        var faultXml = $"""
                        
                                <soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
                                    <soap:Body>
                                        <soap:Fault>
                                            <faultcode>soap:Server</faultcode>
                                            <faultstring>{SecurityElement.Escape(message)}</faultstring>
                                        </soap:Fault>
                                    </soap:Body>
                                </soap:Envelope>
                        """;

        // Отправляем ответ
        SendResponse(response, faultXml);
        response.StatusCode = 500; // Internal Server Error
    }
    
    private static void HandleGetAllBooks(HttpListenerResponse response)
    {
        var booksXml = new StringBuilder();
        foreach (var book in Books)
        {
            booksXml.Append($"""
                             
                                         <Book>
                                             <id>{book.Id}</id>
                                             <title>{book.Title}</title>
                                             <author>{book.Author}</author>
                                         </Book>
                             """);
        }

        var soapResponse = $"""
                                    <soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
                                        <soap:Body>
                                            <Books xmlns="http://tempuri.org/">
                                                {booksXml}
                                            </Books>
                                        </soap:Body>
                                    </soap:Envelope>
                            """;

        Console.WriteLine(soapResponse);
        SendResponse(response, soapResponse);
    }
    
    private static void AddCorsHeaders(HttpListenerResponse response)
    {
        response.AddHeader("Access-Control-Allow-Origin", "*");
        response.AddHeader("Access-Control-Allow-Methods", "POST, GET, OPTIONS");
        response.AddHeader("Access-Control-Allow-Headers", "Content-Type, Accept, SOAPAction");
    }
}