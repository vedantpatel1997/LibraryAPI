using AutoMapper;
using LibraryManagement.API.Helper;
using LibraryManagement.API.Modal;
using LibraryManagement.API.Repos;
using LibraryManagement.API.Repos.Models;
using LibraryManagement.API.Services.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryManagement.API.Services.Implimentation
{
    public class CategoryService : ICategoryService
    {
        private readonly LibraryManagementContext _dbContext;
        private readonly IMapper _mapper;

        public CategoryService(LibraryManagementContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<APIResponse<CategoryModal>> Create(CategoryModal category)
        {
            var response = new APIResponse<CategoryModal>();
            try
            {
                var data = _mapper.Map<CategoryModal, Category>(category);
                await _dbContext.AddAsync(data);
                await _dbContext.SaveChangesAsync();

                response.Data = _mapper.Map<Category, CategoryModal>(data);
                response.IsSuccess = true;
                response.ResponseCode = 201; // Created
            }
            catch (Exception ex)
            {
                response.ErrorMessage = ex.Message;
                response.ResponseCode = 500; // Internal Server Error
            }
            return response;
        }

        public async Task<APIResponse<List<CategoryModal>>> GetAll()
        {
            var response = new APIResponse<List<CategoryModal>>();
            try
            {
                var data = await _dbContext.Categories.ToListAsync();
                response.Data = _mapper.Map<List<Category>, List<CategoryModal>>(data);
                response.IsSuccess = true;
                response.ResponseCode = 200;
            }
            catch (Exception ex)
            {
                response.ErrorMessage = ex.Message;
                response.ResponseCode = 500; // Internal Server Error
            }
            return response;
        }

        public async Task<APIResponse<CategoryModal>> GetById(int categoryId)
        {
            var response = new APIResponse<CategoryModal>();
            try
            {
                var data = await _dbContext.Categories.FirstOrDefaultAsync(c => c.CategoryId == categoryId);
                if (data != null)
                {
                    response.Data = _mapper.Map<Category, CategoryModal>(data);
                    response.IsSuccess = true;
                    response.ResponseCode = 200;
                }
                else
                {
                    response.ResponseCode = 404; // Not Found
                    response.ErrorMessage = "Data not found";
                }
            }
            catch (Exception ex)
            {
                response.ErrorMessage = ex.Message;
                response.ResponseCode = 500; // Internal Server Error
            }
            return response;
        }

        public async Task<APIResponse> Remove(int categoryId)
        {
            var response = new APIResponse();
            try
            {
                var category = await _dbContext.Categories.SingleOrDefaultAsync(c => c.CategoryId == categoryId);

                if (category == null)
                {
                    response.ResponseCode = 404; // Not Found
                    response.ErrorMessage = "Data not found";
                    return response;
                }

                // Check if the category has associated books
                var hasAssociatedBooks = _dbContext.Books.Any(b => b.CategoryId == categoryId);

                if (hasAssociatedBooks)
                {
                    // Category has associated books, generate an error response
                    response.ResponseCode = 403; // Forbidden
                    response.ErrorMessage = "This category has associated books and cannot be deleted.";
                }
                else
                {
                    // Category has no associated books, proceed with deletion
                    _dbContext.Remove(category);
                    await _dbContext.SaveChangesAsync();
                    response.IsSuccess = true;
                    response.ResponseCode = 204; // No Content (successful delete)
                }
            }
            catch (Exception ex)
            {
                response.ErrorMessage = ex.Message;
                response.ResponseCode = 500; // Internal Server Error
            }
            return response;
        }

        public async Task<APIResponse> Update(CategoryModal category, int categoryId)
        {
            var response = new APIResponse();
            try
            {
                var existingCategory = await _dbContext.Categories.FirstOrDefaultAsync(c => c.CategoryId == categoryId);
                if (existingCategory != null)
                {
                    _mapper.Map(category, existingCategory);
                    await _dbContext.SaveChangesAsync();
                    response.IsSuccess = true;
                    response.ResponseCode = 200;
                }
                else
                {
                    response.ResponseCode = 404; // Not Found
                    response.ErrorMessage = "Data not found";
                }
            }
            catch (Exception ex)
            {
                response.ErrorMessage = ex.Message;
                response.ResponseCode = 500; // Internal Server Error
            }
            return response;
        }
    }
}
